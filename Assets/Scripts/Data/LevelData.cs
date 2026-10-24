using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnLocation
{
    Random,
    Left,
    Right,
    Top
}

public enum WaveStartCondition
{
    RoundStart,
    FinishedSpawning,
    AllDefeated
}

[Serializable]
public class SpawnEntry
{
    public EnemyData enemy_data;
    public int count = 1;
    public SpawnLocation spawn_location = SpawnLocation.Random;
    public float delay_before = 5f;
    public float delay_between = 1f;
}

[Serializable]
public class WaveData
{
    public int wave_number = 1;
    public bool is_huge_wave = false;
    public WaveStartCondition start_condition = WaveStartCondition.RoundStart;
    public List<SpawnEntry> spawns = new List<SpawnEntry>();
}

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Detergentnation/Level Data")]
public class LevelData : ScriptableObject
{
    public int level_number = 1;
    public int defense_points_start = 6;
    public int max_restocks_for_star = 3;
    public int max_defense_points_for_star = 6;
    public int min_minimarket_hp_percent_for_star = 50;
    public int target_kill_streak_for_star = 10;
    public List<WaveData> waves = new List<WaveData>();
}
