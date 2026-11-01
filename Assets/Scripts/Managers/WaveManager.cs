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

    [Header("Wave Break Config")]
    [SerializeField] private float default_break_duration = 20f;
    public UnityEvent<float> on_break_started; // duration
    public UnityEvent<float> on_break_timer_tick; // remaining_seconds
    public UnityEvent on_break_ended;

    public AudioClip huge_wave_sound;
    public UnityEvent<int, int> on_wave_started; // current_wave, total_waves
    public UnityEvent<bool> on_huge_wave_alert;
    public UnityEvent on_all_waves_cleared;

    private LevelData current_level_data;
    private int current_wave_index = -1;
    private int active_enemies_count = 0;
    private bool skip_break_requested = false;
    private bool is_in_break = false;

    public bool is_running { get; private set; } = false;
    public bool IsInBreak => is_in_break;

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
        if (is_running) return;
        StartCoroutine(WaveRoutine());
    }

    public void SkipBreak()
    {
        if (is_in_break)
        {
            skip_break_requested = true;
        }
    }

    private IEnumerator WaveRoutine()
    {
        if (current_level_data == null || current_level_data.waves.Count == 0) yield break;
        is_running = true;

        for (int i = 0; i < current_level_data.waves.Count; i++)
        {
            current_wave_index = i;
            WaveData wave = current_level_data.waves[i];

            if (wave.is_huge_wave)
            {
                if (huge_wave_sound != null)
                {
                    AudioManager.PlaySFX(huge_wave_sound);
                }
                else
                {
                    AudioManager.PlaySFX("huge_wave_incoming");
                }
                if (on_huge_wave_alert != null)
                {
                    on_huge_wave_alert.Invoke(true);
                }
                NotificationToast.Show("WARNING: Huge Wave Incoming!", ToastType.Warning);
            }
            else
            {
                NotificationToast.Show("Wave " + (current_wave_index + 1) + " / " + current_level_data.waves.Count + " Started!", ToastType.Info);
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

            // Wait until all enemies of the current wave are defeated
            while (active_enemies_count > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }

            // If there are more waves left, trigger Break Time
            bool has_more_waves = (i + 1) < current_level_data.waves.Count;
            if (has_more_waves)
            {
                yield return StartCoroutine(BreakRoutine(default_break_duration));
            }
        }

        NotificationToast.Show("All waves cleared!", ToastType.Success);
        is_running = false;

        if (on_all_waves_cleared != null)
        {
            on_all_waves_cleared.Invoke();
        }
    }

    private IEnumerator BreakRoutine(float duration)
    {
        is_in_break = true;
        skip_break_requested = false;

        if (LevelManager.instance != null)
        {
            LevelManager.instance.StartWaveBreak();
        }

        AudioManager.PlaySFX("victory_small");
        NotificationToast.Show("Wave Cleared! Break Time: Restock & Repair!", ToastType.Success);

        on_break_started?.Invoke(duration);

        float timer = duration;
        while (timer > 0f && !skip_break_requested)
        {
            on_break_timer_tick?.Invoke(timer);
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        on_break_timer_tick?.Invoke(0f);
        on_break_ended?.Invoke();

        is_in_break = false;
        skip_break_requested = false;

        if (LevelManager.instance != null)
        {
            LevelManager.instance.StartCombatPhase();
        }
    }

    private void SpawnEnemy(EnemyData data, SpawnLocation location)
    {
        if (data == null) return;

        GameObject prefab = data.enemy_prefab != null ? data.enemy_prefab : enemy_prefab;
        if (prefab == null) return;

        Vector3 spawn_pos = GetSpawnPosition(location, data.is_flying);
        GameObject obj = Instantiate(prefab, spawn_pos, Quaternion.identity);

        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.enemy_data = data;
        }

        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
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

    public void OnEnemyKilled(EnemyBase enemy)
    {
        active_enemies_count--;
        if (active_enemies_count < 0) active_enemies_count = 0;
    }

    public int GetActiveEnemiesCount()
    {
        return active_enemies_count;
    }
}
