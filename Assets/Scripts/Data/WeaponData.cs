using UnityEngine;

public enum WeaponType
{
    Ranged,
    Melee
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Detergentnation/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weapon_name;
    public WeaponType weapon_type;
    public int price = 0;
    public int damage = 15;
    public float fire_rate = 0.3f;
    public float attack_rate = 0.4f;
    public int magazine_capacity = 12;
    public int carried_ammo_base = 64;
    public float projectile_speed = 12f;
    public float attack_range = 1.5f;
    public int pellet_count = 1;
    public float spread_angle = 0f;
    public bool is_floating_bubble = false;
    public float projectile_lifetime = 3f;
    public GameObject projectile_prefab;
    public GameObject weapon_prefab;
    public AudioClip attack_sound;
}
