using UnityEngine;
using UnityEngine.InputSystem;

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

    // Components
    private PlayerWeaponAim weapon_aim;
    private PlayerAmmo ammo_comp;
    private PlayerConsumableInventory consumable_inv;

    // Weapon Instances
    private GameObject current_weapon_instance;
    private RangedWeapon active_ranged_comp;
    private MeleeWeapon active_melee_comp;

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

        if (weapon_mount == transform)
        {
            weapon_mount = null;
        }

        if (weapon_mount == null)
        {
            Transform found = transform.Find("WeaponMount");
            if (found == null) found = transform.Find("Visuals/GunArm");
            if (found == null && weapon_aim != null)
            {
                weapon_aim.EnsureWeaponMount();
                found = weapon_aim.weapon_mount;
            }
            weapon_mount = found;
        }
        if (weapon_aim != null)
        {
            if (weapon_mount != null) weapon_aim.weapon_mount = weapon_mount;
            else weapon_mount = weapon_aim.weapon_mount;
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
        if (ranged_weapon == null || active_ranged_comp == null) return;
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

        active_ranged_comp.Fire(aim_dir, weapon_damage_bonus);
    }

    private void PerformMelee()
    {
        if (melee_weapon == null || active_melee_comp == null) return;

        next_melee_time = Time.time + melee_weapon.attack_rate;

        if (melee_weapon.attack_sound != null)
        {
            AudioManager.PlaySFX(melee_weapon.attack_sound);
        }

        Vector3 origin = melee_point != null ? melee_point.position : transform.position;
        active_melee_comp.PerformStrike(origin, melee_weapon.attack_range, melee_weapon.damage + weapon_damage_bonus, enemy_layer);
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
        if (weapon_mount == transform)
        {
            weapon_mount = null;
        }

        if (weapon_mount == null)
        {
            Transform found = transform.Find("WeaponMount");
            if (found == null) found = transform.Find("Visuals/GunArm");
            if (found == null && weapon_aim != null)
            {
                weapon_aim.EnsureWeaponMount();
                found = weapon_aim.weapon_mount;
            }
            weapon_mount = found;
        }

        if (weapon_mount == null || weapon_mount == transform) return;

        if (current_weapon_instance != null)
        {
            Destroy(current_weapon_instance);
            current_weapon_instance = null;
            active_ranged_comp = null;
            active_melee_comp = null;
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

            active_ranged_comp = current_weapon_instance.GetComponent<RangedWeapon>();
            active_melee_comp = current_weapon_instance.GetComponent<MeleeWeapon>();

            Transform fp = current_weapon_instance.transform.Find("FirePoint");
            if (fp != null) fire_point = fp;

            Transform mp = current_weapon_instance.transform.Find("MeleePoint");
            if (mp != null) melee_point = mp;
        }
    }
}
