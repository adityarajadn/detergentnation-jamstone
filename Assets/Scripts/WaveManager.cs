using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Transform leftPortal;
    public Transform rightPortal;
    public Transform topPortal;
    public GameObject enemyBasePrefab;

    [System.Serializable]
    public struct EnemyPrefabMap
    {
        public string type;
        public GameObject prefab;
    }
    public EnemyPrefabMap[] enemyPrefabs;

    public LevelConfigSO[] allLevels;
    public LevelConfigSO currentLevel;

    public bool autoStartOnAwake = true;
    public int autoStartLevelId = 1;

    private int currentWaveIdx = 0;
    private int activeEnemies = 0;
    private int totalEnemiesThisLevel = 0;
    private int defeatedEnemies = 0;

    private int killStreak = 0;
    private int maxKillStreak = 0;

    public event Action<int, int, bool> OnWaveStarted;
    public event Action<float> OnProgressUpdated;
    public event Action<int, int> OnKillStreakUpdated;
    public event Action<int> OnLevelCompleted;

    private void Start()
    {
        if (autoStartOnAwake)
        {
            StartLevel(autoStartLevelId);
        }
    }

    private void OnEnable() => EventBus.OnEnemyKilled += HandleEnemyKilled;
    private void OnDisable() => EventBus.OnEnemyKilled -= HandleEnemyKilled;

    public void StartLevel(int levelId)
    {
        if (allLevels == null || allLevels.Length == 0) return;
        currentLevel = allLevels[Mathf.Clamp(levelId - 1, 0, allLevels.Length - 1)];
        currentWaveIdx = 0;
        defeatedEnemies = 0;
        killStreak = 0;
        maxKillStreak = 0;

        totalEnemiesThisLevel = 0;
        foreach (var wave in currentLevel.waves)
            foreach (var grp in wave.enemyGroups)
                totalEnemiesThisLevel += grp.count;

        StartCoroutine(RunWaveRoutine());
    }

    private IEnumerator RunWaveRoutine()
    {
        while (currentWaveIdx < currentLevel.waves.Count)
        {
            var wave = currentLevel.waves[currentWaveIdx];
            yield return new WaitForSeconds(wave.preDelay);

            OnWaveStarted?.Invoke(currentWaveIdx + 1, currentLevel.waves.Count, wave.isHugeWave);
            if (wave.isHugeWave) AudioManager.Instance?.PlaySfx("alarm");

            foreach (var grp in wave.enemyGroups)
            {
                for (int i = 0; i < grp.count; i++)
                {
                    SpawnEnemy(grp.enemyType, grp.spawnLocation);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }

            while (activeEnemies > 0) yield return null;
            currentWaveIdx++;
        }

        EvaluateVictory();
    }

    private void SpawnEnemy(string type, string location)
    {
        Vector3 pos = location switch
        {
            "left" => leftPortal != null ? leftPortal.position : new Vector3(-8, 0, 0),
            "right" => rightPortal != null ? rightPortal.position : new Vector3(8, 0, 0),
            "top" => topPortal != null ? topPortal.position : new Vector3(0, 5, 0),
            _ => (UnityEngine.Random.value < 0.5f ? (leftPortal != null ? leftPortal.position : new Vector3(-8, 0, 0)) : (rightPortal != null ? rightPortal.position : new Vector3(8, 0, 0)))
        };

        GameObject prefabToSpawn = enemyBasePrefab;
        if (enemyPrefabs != null)
        {
            foreach (var map in enemyPrefabs)
            {
                if (map.prefab != null && string.Equals(map.type, type, StringComparison.OrdinalIgnoreCase))
                {
                    prefabToSpawn = map.prefab;
                    break;
                }
            }
        }

        if (prefabToSpawn != null)
        {
            GameObject enemy = Instantiate(prefabToSpawn, pos, Quaternion.identity);
            var baseComp = enemy.GetComponent<EnemyBase>();
            if (baseComp != null && string.IsNullOrEmpty(baseComp.enemyType)) baseComp.enemyType = type;
            activeEnemies++;
        }
    }

    private void HandleEnemyKilled(string type, Vector2 pos)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
        defeatedEnemies++;
        OnProgressUpdated?.Invoke((float)defeatedEnemies / Mathf.Max(1, totalEnemiesThisLevel));

        killStreak++;
        maxKillStreak = Mathf.Max(maxKillStreak, killStreak);

        int bonus = killStreak switch
        {
            5 => 50,
            10 => 150,
            20 => 400,
            40 => 1000,
            _ => 0
        };

        if (bonus > 0)
        {
            GameState.Instance?.AddCandy(bonus);
            AudioManager.Instance?.PlaySfx("streak");
        }

        OnKillStreakUpdated?.Invoke(killStreak, bonus);
    }

    private void EvaluateVictory()
    {
        int stars = 1;
        var store = FindAnyObjectByType<Minimarket>();
        if (store != null)
        {
            float hpPct = (float)store.healthCurrent / store.healthMax;
            if (hpPct >= 0.5f) stars = 2;
            if (hpPct >= 0.7f && maxKillStreak >= 5) stars = 3;
        }

        if (currentLevel != null && GameState.Instance != null)
        {
            GameState.Instance.RecordLevelResult(currentLevel.levelId, stars);
        }
        OnLevelCompleted?.Invoke(stars);
        EventBus.RaiseLevelCompleted(stars);
    }
}
