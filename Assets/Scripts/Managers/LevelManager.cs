using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GamePhase
{
    Planning,
    Combat,
    Victory,
    Defeat
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public GamePhase current_phase = GamePhase.Planning;
    public List<LevelData> level_database = new List<LevelData>();
    public LevelData active_level_data;

    public UnityEvent<GamePhase> on_phase_changed;
    public UnityEvent<int> on_victory; // stars earned
    public UnityEvent on_defeat;

    private int restock_count = 0;
    private int stars_earned = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AudioManager.PlayMusic("ingame_bgm");

        int level_to_load = GameManager.selected_level;
        if (level_database.Count > 0)
        {
            int index = Mathf.Clamp(level_to_load - 1, 0, level_database.Count - 1);
            active_level_data = level_database[index];
        }

        if (active_level_data != null)
        {
            SetupEnvironment();

            if (DefensePlacementManager.instance != null)
            {
                DefensePlacementManager.instance.InitPoints(active_level_data.defense_points_start);
            }
            if (WaveManager.instance != null)
            {
                WaveManager.instance.InitLevel(active_level_data);
                WaveManager.instance.on_all_waves_cleared.AddListener(OnLevelVictory);
            }
        }

        Health player_health = PlayerController.Instance != null ? PlayerController.Instance.GetComponent<Health>() : null;
        if (player_health == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
            if (player != null) player_health = player.GetComponent<Health>();
        }

        if (player_health != null)
        {
            // Player death sequence will invoke OnLevelDefeat after animation and equipment fling finish
            player_health.on_death.AddListener(() =>
            {
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.TriggerDeathSequence(OnLevelDefeat);
                }
                else
                {
                    OnLevelDefeat();
                }
            });
        }

        if (Minimarket.instance != null && Minimarket.instance.health != null)
        {
            Minimarket.instance.health.on_death.AddListener(OnLevelDefeat);
        }

        SetPhase(GamePhase.Planning);
    }

    public void StartCombatPhase()
    {
        if (current_phase != GamePhase.Planning) return;

        SetPhase(GamePhase.Combat);
        if (WaveManager.instance != null)
        {
            WaveManager.instance.StartWaves();
        }
    }

    public void RegisterRestock()
    {
        restock_count++;
    }

    private void OnLevelVictory()
    {
        if (current_phase == GamePhase.Defeat) return;

        stars_earned = CalculateStars();
        SetPhase(GamePhase.Victory);

        if (GameManager.instance != null && active_level_data != null)
        {
            GameManager.instance.CompleteLevel(active_level_data.level_number, stars_earned);
        }

        if (on_victory != null)
        {
            on_victory.Invoke(stars_earned);
        }
        AudioManager.PlaySFX("victory");
    }

    private void OnLevelDefeat()
    {
        if (current_phase == GamePhase.Victory) return;

        SetPhase(GamePhase.Defeat);
        if (on_defeat != null)
        {
            on_defeat.Invoke();
        }
        AudioManager.PlaySFX("defeat");
    }

    private int CalculateStars()
    {
        int stars = 1; // base star for completing level

        if (active_level_data == null) return stars;

        if (Minimarket.instance != null && Minimarket.instance.health != null)
        {
            float hp_ratio = (float)Minimarket.instance.health.health_current / Minimarket.instance.health.health_max;
            if (hp_ratio * 100f >= active_level_data.min_minimarket_hp_percent_for_star)
            {
                stars++;
            }
        }

        if (KillStreakManager.instance != null)
        {
            if (KillStreakManager.instance.GetHighestStreak() >= active_level_data.target_kill_streak_for_star)
            {
                stars++;
            }
        }

        return Mathf.Clamp(stars, 1, 3);
    }

    private void SetPhase(GamePhase phase)
    {
        current_phase = phase;
        if (on_phase_changed != null)
        {
            on_phase_changed.Invoke(current_phase);
        }
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadGameplay(GameManager.selected_level);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = GameManager.selected_level + 1;
        if (next > 5) next = 1;
        SceneLoader.LoadGameplay(next);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadMainMenu();
    }

    private void SetupEnvironment()
    {
        if (active_level_data == null || active_level_data.environment_prefab == null) return;

        GameObject existingEnv = GameObject.Find("Environment_Root");
        if (existingEnv != null)
        {
            Destroy(existingEnv);
        }
        else
        {
            GameObject pb = GameObject.Find("ParallaxBackground");
            if (pb != null) Destroy(pb);
            GameObject grid = GameObject.Find("Grid");
            if (grid != null) Destroy(grid);
        }

        GameObject envInstance = Instantiate(active_level_data.environment_prefab, Vector3.zero, Quaternion.identity);
        envInstance.name = "Environment_Root";
    }
}
