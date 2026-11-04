using UnityEngine;
using UnityEngine.Events;

public class KillStreakManager : MonoBehaviour
{
    public static KillStreakManager instance;

    [System.Serializable]
    public struct StreakReward
    {
        public int count;
        public int bonus_candy;
    }

    public int streak_count = 0;
    public float streak_timeout = 5f;
    public UnityEvent<int, int> on_streak_changed; // current_streak, bonus_candy
    public UnityEvent on_streak_reset;

    [Header("Streak Rewards")]
    [SerializeField] private StreakReward[] streak_tiers = new StreakReward[]
    {
        new StreakReward { count = 5, bonus_candy = 50 },
        new StreakReward { count = 10, bonus_candy = 150 },
        new StreakReward { count = 20, bonus_candy = 400 },
        new StreakReward { count = 40, bonus_candy = 1000 }
    };

    private float timer = 0f;
    private int highest_streak_achieved = 0;

    private Health cached_player_health;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        cached_player_health = PlayerController.Instance != null ? PlayerController.Instance.GetComponent<Health>() : null;
        if (cached_player_health != null)
        {
            cached_player_health.on_health_changed.AddListener(OnPlayerHealthChanged);
        }
    }

    private void OnDestroy()
    {
        if (cached_player_health != null)
        {
            cached_player_health.on_health_changed.RemoveListener(OnPlayerHealthChanged);
        }
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (streak_count > 0)
        {
            timer += Time.deltaTime;
            if (timer >= streak_timeout)
            {
                ResetStreak();
            }
        }
    }

    public void RegisterKill()
    {
        streak_count++;
        timer = 0f;

        if (streak_count > highest_streak_achieved)
        {
            highest_streak_achieved = streak_count;
        }

        int bonus = CalculateKillStreakBonus(streak_count);
        if (bonus > 0 && GameManager.instance != null)
        {
            GameManager.instance.AddCandy(bonus);
        }

        if (on_streak_changed != null)
        {
            on_streak_changed.Invoke(streak_count, bonus);
        }
    }

    private int CalculateKillStreakBonus(int count)
    {
        if (streak_tiers != null)
        {
            for (int i = 0; i < streak_tiers.Length; i++)
            {
                if (streak_tiers[i].count == count) return streak_tiers[i].bonus_candy;
            }
        }
        return 0;
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        ResetStreak();
    }

    public void ResetStreak()
    {
        streak_count = 0;
        timer = 0f;
        if (on_streak_reset != null)
        {
            on_streak_reset.Invoke();
        }
    }

    public int GetHighestStreak()
    {
        return highest_streak_achieved;
    }
}
