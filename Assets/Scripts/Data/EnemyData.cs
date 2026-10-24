using UnityEngine;

public enum EnemyType
{
    Normal,
    Tiny,
    Brawler,
    Brute,
    Tanker
}

public enum EnemyAttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Detergentnation/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemy_name;
    public EnemyType enemy_type;
    public EnemyAttackType attack_type;
    public bool is_flying;
    public int health_max = 50;
    public float move_speed = 2f;
    public int attack_damage = 10;
    public float attack_rate = 1f;
    public float attack_range = 1f;
    public float aggro_range = 6f;
    public int candy_reward_min = 1;
    public int candy_reward_max = 3;
    public GameObject projectile_prefab;
    public Color body_color = Color.white;
}
