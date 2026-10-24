using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [SerializeField] private Transform portal_left;
    [SerializeField] private Transform portal_right;
    [SerializeField] private Transform portal_top;
    [SerializeField] private GameObject enemy_prefab;

    public AudioClip huge_wave_sound;
    public UnityEvent<int, int> on_wave_started; // current_wave, total_waves
    public UnityEvent<bool> on_huge_wave_alert;
    public UnityEvent on_all_waves_cleared;

    private LevelData current_level_data;
    private int current_wave_index = -1;
    private int active_enemies_count = 0;

    private void Awake()
    {
        instance = this;
    }

    public void InitLevel(LevelData level_data)
    {
        current_level_data = level_data;
        current_wave_index = -1;
        active_enemies_count = 0;
    }

    public void StartWaves()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        if (current_level_data == null || current_level_data.waves.Count == 0) yield break;

        for (int i = 0; i < current_level_data.waves.Count; i++)
        {
            current_wave_index = i;
            WaveData wave = current_level_data.waves[i];

            if (wave.start_condition == WaveStartCondition.AllDefeated)
            {
                while (active_enemies_count > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (wave.is_huge_wave)
            {
                if (huge_wave_sound != null)
                {
                    AudioManager.PlaySFX(huge_wave_sound);
                }
                if (on_huge_wave_alert != null)
                {
                    on_huge_wave_alert.Invoke(true);
                }
            }

            if (on_wave_started != null)
            {
                on_wave_started.Invoke(current_wave_index + 1, current_level_data.waves.Count);
            }

            for (int s = 0; s < wave.spawns.Count; s++)
            {
                SpawnEntry entry = wave.spawns[s];
                yield return new WaitForSeconds(entry.delay_before);

                for (int c = 0; c < entry.count; c++)
                {
                    SpawnEnemy(entry.enemy_data, entry.spawn_location);
                    if (entry.delay_between > 0f)
                    {
                        yield return new WaitForSeconds(entry.delay_between);
                    }
                }
            }
        }

        while (active_enemies_count > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (on_all_waves_cleared != null)
        {
            on_all_waves_cleared.Invoke();
        }
    }

    private void SpawnEnemy(EnemyData data, SpawnLocation location)
    {
        if (data == null || enemy_prefab == null) return;

        Vector3 spawn_pos = GetSpawnPosition(location, data.is_flying);
        GameObject obj = Instantiate(enemy_prefab, spawn_pos, Quaternion.identity);

        EnemyController enemy = obj.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.enemy_data = data;
        }

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = data.body_color;
        }

        active_enemies_count++;
    }

    private Vector3 GetSpawnPosition(SpawnLocation loc, bool is_flying)
    {
        if (loc == SpawnLocation.Random)
        {
            int r = is_flying ? Random.Range(0, 3) : Random.Range(0, 2);
            loc = r == 0 ? SpawnLocation.Left : (r == 1 ? SpawnLocation.Right : SpawnLocation.Top);
        }

        if (loc == SpawnLocation.Left && portal_left != null) return portal_left.position;
        if (loc == SpawnLocation.Right && portal_right != null) return portal_right.position;
        if (loc == SpawnLocation.Top && portal_top != null) return portal_top.position;

        return Vector3.zero;
    }

    public void OnEnemyKilled(EnemyController enemy)
    {
        active_enemies_count--;
        if (active_enemies_count < 0) active_enemies_count = 0;
    }

    public int GetActiveEnemiesCount()
    {
        return active_enemies_count;
    }
}
