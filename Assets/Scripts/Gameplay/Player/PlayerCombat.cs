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
    public Transform hand_mount;
    public Transform weapon_attachment;
    public LayerMask enemy_layer;


    [Header("Resources")]
    public int repair_points = 0;

    [Header("Upgrade Scaling")]
    [SerializeField] private int damage_bonus_per_level = 3;

    // Components
    private PlayerWeaponAim weapon_aim;
    private PlayerAmmo ammo_comp;

    // Weapon Instances
    private GameObject current_weapon_instance;
    private RangedWeapon active_ranged_comp;
    private MeleeWeapon active_melee_comp;

    public WeaponBase active_weapon => active_weapon_type == WeaponType.Melee ? (WeaponBase)active_melee_comp : (WeaponBase)active_ranged_comp;

    private float next_fire_time = 0f;
    private float next_melee_time = 0f;
    private Camera main_camera;
    private int weapon_damage_bonus = 0;
    private float fire_rate_mod = 1f;
    private Animator hand_animator;
    private Coroutine melee_attack_coroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ServiceLocator.Register<PlayerCombat>(this);

        weapon_aim = GetComponent<PlayerWeaponAim>();
        ammo_comp = GetComponent<PlayerAmmo>();
        main_camera = Camera.main;
        if (enemy_layer.value == 0)
        {
            enemy_layer = LayerMask.GetMask(GameConstants.Layers.Enemy);
        }

        if (hand_mount != null)
        {
            Transform hand = hand_mount.Find("Hand");
            if (hand != null) hand_animator = hand.GetComponent<Animator>();
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

        int fire_rate_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(GameConstants.UpgradeKeys.WeaponFireRate) : 1;
        fire_rate_mod = Mathf.Max(0.5f, 1f - (fire_rate_level - 1) * 0.05f);

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
        if (Time.timeScale == 0f) return;
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
            if (ammo_comp != null && ammo_comp.ammo_carried <= 0 && ammo_comp.ammo_in_mag <= 0)
            {
                NotificationToast.Show("Out of ammo! Restock at Minimarket!", ToastType.Warning);
            }
            Reload();
            return;
        }

        ammo_comp.ConsumeBullet();
        next_fire_time = Time.time + (ranged_weapon.fire_rate * fire_rate_mod);

        if (ranged_weapon.attack_sound != null)
        {
            AudioManager.PlaySFX(ranged_weapon.attack_sound);
        }
        else
        {
            AudioManager.PlaySFX(ranged_weapon.attack_sound_key);
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
        else
        {
            AudioManager.PlaySFX(melee_weapon.attack_sound_key);
        }

        // Hand attack animation is chosen by the weapon data (attack_style)
        bool has_hand_animation = GameConstants.AnimParams.TryGetMeleeTrigger(melee_weapon.attack_style, out int attack_trigger);

        // For weapons without animated hand colliders, use raycast/overlap strike
        if (!has_hand_animation)
        {
            Vector3 origin = melee_point != null ? melee_point.position : transform.position;
            active_melee_comp.PerformStrike(origin, melee_weapon.attack_range, melee_weapon.damage + weapon_damage_bonus, enemy_layer);
        }
        else
        {
            if (hand_animator == null && hand_mount != null)
            {
                Transform hand = hand_mount.Find("Hand");
                if (hand != null) hand_animator = hand.GetComponent<Animator>();
            }

            if (hand_animator != null)
            {
                hand_animator.SetTrigger(attack_trigger);
            }

            if (weapon_attachment != null)
            {
                if (melee_attack_coroutine != null) StopCoroutine(melee_attack_coroutine);
                melee_attack_coroutine = StartCoroutine(HideWeaponDuringAttack(0.33f));
            }
        }
    }

    private System.Collections.IEnumerator HideWeaponDuringAttack(float duration)
    {
        weapon_attachment.gameObject.SetActive(false);
        yield return new WaitForSeconds(duration);
        if (weapon_attachment != null)
        {
            weapon_attachment.gameObject.SetActive(true);
        }
        melee_attack_coroutine = null;
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
        if (hand_mount == null && weapon_aim != null)
        {
            hand_mount = weapon_aim.hand_mount;
        }

        if (hand_mount == null || hand_mount == transform) return;

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
            SpriteRenderer arm_sr = hand_mount.GetComponentInChildren<SpriteRenderer>();
            if (arm_sr != null) arm_sr.enabled = true;

            Transform parent_target = weapon_attachment != null ? weapon_attachment : hand_mount;
            current_weapon_instance = Instantiate(active_data.weapon_prefab, parent_target);
            current_weapon_instance.transform.localPosition = Vector3.zero;
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

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerCombat>();
        if (Instance == this) Instance = null;
    }
}
