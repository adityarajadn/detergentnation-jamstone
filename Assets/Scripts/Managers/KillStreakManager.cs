using UnityEngine;
using UnityEngine.Events;

public class KillStreakManager : MonoBehaviour
{
    public static KillStreakManager instance;

    public int streak_count = 0;
    public float streak_timeout = 5f;
    public UnityEvent<int, int> on_streak_changed; // current_streak, bonus_candy
    public UnityEvent on_streak_reset;

    private float timer = 0f;
    private int highest_streak_achieved = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health player_health = player.GetComponent<Health>();
            if (player_health != null)
            {
                player_health.on_health_changed.AddListener(OnPlayerHealthChanged);
            }
        }
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
        if (count == 5) return 50;
        if (count == 10) return 150;
        if (count == 20) return 400;
        if (count == 40) return 1000;
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
