using System;
using UnityEngine;

public static class EventBus
{
    public static event Action<int> OnCurrencyChanged;
    public static event Action OnPlayerDied;
    public static event Action OnPlayerDamaged;
    public static event Action<int> OnLevelCompleted;
    public static event Action<string> OnLevelFailed;
    public static event Action<int, int, bool> OnWaveStarted;
    public static event Action<string, Vector2> OnEnemyKilled;

    public static void RaiseCurrencyChanged(int amount) => OnCurrencyChanged?.Invoke(amount);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
    public static void RaisePlayerDamaged() => OnPlayerDamaged?.Invoke();
    public static void RaiseLevelCompleted(int stars) => OnLevelCompleted?.Invoke(stars);
    public static void RaiseLevelFailed(string reason) => OnLevelFailed?.Invoke(reason);
    public static void RaiseWaveStarted(int wave, int total, bool huge) => OnWaveStarted?.Invoke(wave, total, huge);
    public static void RaiseEnemyKilled(string type, Vector2 pos) => OnEnemyKilled?.Invoke(type, pos);
}
