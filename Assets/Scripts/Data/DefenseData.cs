using UnityEngine;

public enum DefenseType
{
    HealthBased,
    AmmoBased,
    SingleUse
}

[CreateAssetMenu(fileName = "NewDefenseData", menuName = "Detergentnation/Defense Data")]
public class DefenseData : ScriptableObject
{
    public string defense_name;
    public DefenseType defense_type;
    public int point_cost = 1;
    public int max_health = 100;
    public int max_ammo = 10;
    public int damage = 25;
    public float attack_range = 5f;
    public GameObject prefab;
    public Sprite icon;
}
