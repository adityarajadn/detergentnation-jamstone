using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "DetergentNation/Audio/AudioDatabase")]
public class AudioDatabase : ScriptableObject
{
    [Header("1-4. Title & Menu")]
    public AudioClip title_select;
    public AudioClip game_start;
    public AudioClip scene_transition;
    public AudioClip main_menu_bgm;

    [Header("5-8. Ranged Shoot")]
    public AudioClip shoot_water_pistol;
    public AudioClip shoot_water_rifle;
    public AudioClip shoot_water_plasma;
    public AudioClip shoot_bubble_gun;

    [Header("9-11. Melee Swing")]
    public AudioClip swing_soap_hammer;
    public AudioClip swing_bubble_stick;
    public AudioClip swing_curse_sword;

    [Header("12-14. Melee Hit")]
    public AudioClip hit_soap_hammer;
    public AudioClip hit_bubble_stick;
    public AudioClip hit_curse_sword;

    [Header("15. In-Game BGM")]
    public AudioClip ingame_bgm;

    [Header("16-19. Enemy SFX")]
    public AudioClip enemy_death;
    public AudioClip enemy_hurt;
    public AudioClip enemy_walk_heavy;
    public AudioClip enemy_attack;

    [Header("20-22. Fence")]
    public AudioClip fence_break;
    public AudioClip fence_place;
    public AudioClip fence_hurt;

    [Header("23-24. Trap")]
    public AudioClip trap_trigger;
    public AudioClip trap_set;

    [Header("25-26. Soap Barrel")]
    public AudioClip soap_barrel_roll;
    public AudioClip soap_barrel_break;

    [Header("27-30. Consumables")]
    public AudioClip consumable_healing;
    public AudioClip consumable_drink;
    public AudioClip consumable_whistle;
    public AudioClip consumable_repair;

    [Header("31-33. Jingle & Alert")]
    public AudioClip victory_jingle;
    public AudioClip defeat_jingle;
    public AudioClip huge_wave_alarm;

    [Header("34-36. Player & Shop")]
    public AudioClip player_damage;
    public AudioClip player_death;
    public AudioClip purchase_sound;

    [Header("37-38. Cannon")]
    public AudioClip cannon_fire;
    public AudioClip cannon_impact;
}
