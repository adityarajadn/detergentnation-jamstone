using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerWeaponAim))]
[RequireComponent(typeof(PlayerAmmo))]
[RequireComponent(typeof(PlayerConsumableInventory))]
public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat Instance { get; private set; }

    [Header("Weapons")]
    public WeaponData ranged_weapon;
    public WeaponData melee_weapon;
    public WeaponData[] available_ranged_weapons;
    public WeaponData[] available_melee_weapons;
    public WeaponType active_weapon_type = WeaponType.Ranged;

    [Header("Mount & Targeting")]
    public Transform fire_point;
    public Transform melee_point;
    public Transform weapon_mount;
    public LayerMask enemy_layer;

    [Header("Resources")]
    public int repair_points = 0;

    [Header("Upgrade Scaling")]
    [SerializeField] private int damage_bonus_per_level = 3;

    // Component References
    private PlayerWeaponAim weapon_aim;
    private PlayerAmmo ammo_comp;
    private PlayerConsumableInventory consumable_inv;

    // Backward compatibility proxies
    public int ammo_in_mag => ammo_comp != null ? ammo_comp.ammo_in_mag : 0;
    public int ammo_carried => ammo_comp != null ? ammo_comp.ammo_carried : 0;
    public int ammo_carried_max => ammo_comp != null ? ammo_comp.ammo_carried_max : 0;
    public int active_consumable_slot => consumable_inv != null ? consumable_inv.active_consumable_slot : 1;
    public GameObject soap_bomb_prefab => consumable_inv != null ? consumable_inv.soap_bomb_prefab : null;
    public GameObject sonar_batarang_prefab => consumable_inv != null ? consumable_inv.sonar_batarang_prefab : null;
    public GameObject[] consumable_prefabs
    {
        get => consumable_inv != null ? consumable_inv.consumable_prefabs : null;
        set { if (consumable_inv != null) consumable_inv.consumable_prefabs = value; }
    }
    public UnityEngine.Events.UnityEvent<int, string, int> on_consumable_updated => consumable_inv != null ? consumable_inv.on_consumable_updated : null;

    private GameObject current_weapon_instance;
    private float next_fire_time = 0f;
    private float next_melee_time = 0f;
    private Camera main_camera;
    private int weapon_damage_bonus = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        weapon_aim = GetComponent<PlayerWeaponAim>();
        ammo_comp = GetComponent<PlayerAmmo>();
        consumable_inv = GetComponent<PlayerConsumableInventory>();
        main_camera = Camera.main;

        if (weapon_mount == null)
        {
            Transform found = transform.Find("Visuals/GunArm");
            if (found != null) weapon_mount = found;
            else if (fire_point != null) weapon_mount = fire_point.parent;
        }
        if (weapon_aim != null && weapon_aim.weapon_mount == null)
        {
            weapon_aim.weapon_mount = weapon_mount;
        }
    }

    private void Start()
    {
        InitializeEquipment();
    }

    private void InitializeEquipment()
    {
        string eq_ranged = GameManager.instance != null ? GameManager.instance.equipped_ranged_weapon : GameConstants.EquipmentNames.DefaultRanged;
        if (!string.IsNullOrEmpty(eq_ranged) && available_ranged_weapons != null)
        {
            for (int i = 0; i < available_ranged_weapons.Length; i++)
            {
                if (available_ranged_weapons[i] != null && available_ranged_weapons[i].weapon_name == eq_ranged)
                {
                    ranged_weapon = available_ranged_weapons[i];
                    break;
                }
            }
        }

        string eq_melee = GameManager.instance != null ? GameManager.instance.equipped_melee_weapon : GameConstants.EquipmentNames.DefaultMelee;
        if (!string.IsNullOrEmpty(eq_melee) && available_melee_weapons != null)
        {
            for (int i = 0; i < available_melee_weapons.Length; i++)
            {
                if (available_melee_weapons[i] != null && available_melee_weapons[i].weapon_name == eq_melee)
                {
                    melee_weapon = available_melee_weapons[i];
                    break;
                }
            }
        }

        int dmg_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(GameConstants.UpgradeKeys.WeaponDamage) : 1;
        weapon_damage_bonus = (dmg_level - 1) * damage_bonus_per_level;

        if (ammo_comp != null)
        {
            ammo_comp.Initialize(ranged_weapon);
        }

        active_weapon_type = GameManager.instance != null ? GameManager.instance.active_weapon_type : WeaponType.Ranged;
        UpdateWeaponVisual();
    }

    private void Update()
    {
        HandleCombatInput();
    }

    private void HandleCombatInput()
    {
        bool is_combat = LevelManager.instance == null || LevelManager.instance.current_phase == GamePhase.Combat;
        if (!is_combat) return;

        // Attack -> Auto-switch to Ranged and Shoot
        if (KeybindManager.IsPressed(KeyAction.Attack))
        {
            if (active_weapon_type != WeaponType.Ranged)
            {
                SetActiveWeaponType(WeaponType.Ranged);
            }
            if (Time.time >= next_fire_time)
            {
                ShootRanged();
            }
        }

        // Melee Attack -> Auto-switch to Melee and Strike
        if (KeybindManager.IsPressed(KeyAction.MeleeAttack))
        {
            if (active_weapon_type != WeaponType.Melee)
            {
                SetActiveWeaponType(WeaponType.Melee);
            }
            if (Time.time >= next_melee_time)
            {
                PerformMelee();
            }
        }

        if (KeybindManager.WasPressedThisFrame(KeyAction.Reload))
        {
            Reload();
        }
    }

    public void SetActiveWeaponType(WeaponType new_type)
    {
        if (active_weapon_type == new_type) return;
        active_weapon_type = new_type;
        if (GameManager.instance != null) GameManager.instance.active_weapon_type = active_weapon_type;
        UpdateWeaponVisual();
    }

    private void ShootRanged()
    {
        if (ranged_weapon == null) return;
        if (ammo_comp == null || !ammo_comp.CanShoot())
        {
            Reload();
            return;
        }

        ammo_comp.ConsumeBullet();
        next_fire_time = Time.time + ranged_weapon.fire_rate;

        if (ranged_weapon.attack_sound != null)
        {
            AudioManager.PlaySFX(ranged_weapon.attack_sound);
        }

        if (main_camera == null) main_camera = Camera.main;
        Vector3 mouse_pos = main_camera != null && Mouse.current != null ? main_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) : transform.position;
        mouse_pos.z = 0f;
        Vector3 origin = fire_point != null ? fire_point.position : transform.position;
        Vector2 aim_dir = (mouse_pos - origin).normalized;
        float base_angle = Mathf.Atan2(aim_dir.y, aim_dir.x) * Mathf.Rad2Deg;

        RangedWeapon ranged_comp = current_weapon_instance != null ? current_weapon_instance.GetComponent<RangedWeapon>() : null;
        if (ranged_comp != null)
        {
            ranged_comp.Fire(aim_dir, weapon_damage_bonus);
        }
        else
        {
            int pellets = Mathf.Max(1, ranged_weapon.pellet_count);
            for (int i = 0; i < pellets; i++)
            {
                float angle = base_angle;
                if (pellets > 1)
                {
                    float offset = (i - (pellets - 1) / 2f) * (ranged_weapon.spread_angle / pellets);
                    angle += offset;
                }

                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                if (ranged_weapon.projectile_prefab != null)
                {
                    GameObject proj_obj = Instantiate(ranged_weapon.projectile_prefab, origin, rot);
                    Projectile proj = proj_obj.GetComponent<Projectile>();
                    if (proj != null)
                    {
                        proj.damage = ranged_weapon.damage + weapon_damage_bonus;
                        proj.speed = ranged_weapon.projectile_speed;
                        proj.lifetime = ranged_weapon.projectile_lifetime;
                        proj.is_floating_bubble = ranged_weapon.is_floating_bubble;
                        proj.is_enemy_projectile = false;
                    }
                }
            }
        }
    }

    private void PerformMelee()
    {
        if (melee_weapon == null) return;

        next_melee_time = Time.time + melee_weapon.attack_rate;

        if (melee_weapon.attack_sound != null)
        {
            AudioManager.PlaySFX(melee_weapon.attack_sound);
        }

        Vector3 origin = melee_point != null ? melee_point.position : transform.position;

        MeleeWeapon melee_comp = current_weapon_instance != null ? current_weapon_instance.GetComponent<MeleeWeapon>() : null;
        if (melee_comp != null)
        {
            melee_comp.PerformStrike(origin, melee_weapon.attack_range, melee_weapon.damage + weapon_damage_bonus, enemy_layer);
        }
        else
        {
            Collider2D[] hit_enemies = Physics2D.OverlapCircleAll(origin, melee_weapon.attack_range, enemy_layer);
            for (int i = 0; i < hit_enemies.Length; i++)
            {
                Health enemy_health = hit_enemies[i].GetComponent<Health>();
                if (enemy_health != null)
                {
                    enemy_health.TakeDamage(melee_weapon.damage + weapon_damage_bonus);
                }
            }
        }

        StartCoroutine(MeleeSwingCoroutine());
    }

    private IEnumerator MeleeSwingCoroutine()
    {
        if (current_weapon_instance == null) yield break;
        Transform w_trans = current_weapon_instance.transform;
        float duration = 0.12f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float angle = Mathf.Sin(t * Mathf.PI) * -50f;
            if (w_trans != null)
            {
                w_trans.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
            yield return null;
        }
        if (w_trans != null)
        {
            w_trans.localRotation = Quaternion.identity;
        }
    }

    public void Reload()
    {
        if (ranged_weapon != null && ammo_comp != null)
        {
            ammo_comp.Reload(ranged_weapon.magazine_capacity);
        }
    }

    public void Restock()
    {
        if (ammo_comp != null)
        {
            ammo_comp.Restock();
        }
    }

    public void AddRepairPoints(int amount)
    {
        repair_points += amount;
    }

    public bool UseRepairPoints(int amount)
    {
        if (repair_points < amount) return false;
        repair_points -= amount;
        return true;
    }

    public void UpdateWeaponVisual()
    {
        if (weapon_mount == null)
        {
            Transform found = transform.Find("Visuals/GunArm");
            if (found != null) weapon_mount = found;
            else if (fire_point != null) weapon_mount = fire_point.parent;
        }

        if (weapon_mount == null) return;

        if (current_weapon_instance != null)
        {
            Destroy(current_weapon_instance);
            current_weapon_instance = null;
        }

        WeaponData active_data = active_weapon_type == WeaponType.Melee ? melee_weapon : ranged_weapon;
        if (active_data != null && active_data.weapon_prefab != null)
        {
            SpriteRenderer arm_sr = weapon_mount.GetComponent<SpriteRenderer>();
            if (arm_sr != null) arm_sr.enabled = true;

            current_weapon_instance = Instantiate(active_data.weapon_prefab, weapon_mount);
            current_weapon_instance.transform.localPosition = new Vector3(0.25f, 0f, 0f);
            current_weapon_instance.transform.localRotation = Quaternion.identity;

            WeaponBase wb = current_weapon_instance.GetComponent<WeaponBase>();
            if (wb != null) wb.Initialize(active_data);

            Transform fp = current_weapon_instance.transform.Find("FirePoint");
            if (fp != null) fire_point = fp;

            Transform mp = current_weapon_instance.transform.Find("MeleePoint");
            if (mp != null) melee_point = mp;
        }
    }

    // Backward-compatible consumable proxies
    public string GetActiveConsumableName() => consumable_inv != null ? consumable_inv.GetActiveConsumableName() : "";
    public int GetActiveConsumableCount() => consumable_inv != null ? consumable_inv.GetActiveConsumableCount() : 0;
    public void UseActiveConsumable() { if (consumable_inv != null) consumable_inv.UseActiveConsumable(); }
    public void NotifyConsumableChanged() { if (consumable_inv != null) consumable_inv.NotifyConsumableChanged(); }
}
