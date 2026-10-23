using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnGroup
{
    public string enemyType;
    public string spawnLocation; // "left", "right", "top", "random"
    public int count;
}

[Serializable]
public class WaveData
{
    public bool isHugeWave;
    public float preDelay = 5.0f;
    public float spawnInterval = 1.0f;
    public List<EnemySpawnGroup> enemyGroups = new List<EnemySpawnGroup>();
}

[CreateAssetMenu(fileName = "LevelConfig", menuName = "DetergentNation/LevelConfig")]
public class LevelConfigSO : ScriptableObject
{
    public int levelId = 1;
    public string levelName = "Level 1: First Splash";
    public int defensePoints = 6;
    public List<WaveData> waves = new List<WaveData>();
}
