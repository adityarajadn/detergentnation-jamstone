using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public WeaponData ranged_weapon;
    public WeaponData melee_weapon;
    public WeaponData[] available_ranged_weapons;
    public WeaponData[] available_melee_weapons;
    public WeaponType active_weapon_type = WeaponType.Ranged;

    public int ammo_in_mag = 12;
    public int ammo_carried = 64;
    public int ammo_carried_max = 64;
    public int repair_points = 0;

    public Transform fire_point;
    public Transform melee_point;
    public Transform weapon_mount;
    public LayerMask enemy_layer;

    public int active_consumable_slot = 1;
    public GameObject soap_bomb_prefab;
    public GameObject sonar_batarang_prefab;
    public UnityEngine.Events.UnityEvent<int, string, int> on_consumable_updated;

    private GameObject current_weapon_instance;
    private float next_fire_time = 0f;
    private float next_melee_time = 0f;
    private Camera main_camera;
    private int weapon_damage_bonus = 0;

    private void Start()
    {
        main_camera = Camera.main;

        string eq_ranged = GameManager.instance != null ? GameManager.instance.equipped_ranged_weapon : PlayerPrefs.GetString("equipped_ranged_weapon", "Water Pistol");
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

        string eq_melee = GameManager.instance != null ? GameManager.instance.equipped_melee_weapon : PlayerPrefs.GetString("equipped_melee_weapon", "Bubble Stick");
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

        int dmg_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel("weapon_damage") : PlayerPrefs.GetInt("upgrade_weapon_damage", 1);
        weapon_damage_bonus = (dmg_level - 1) * 3;

        int ammo_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel("weapon_ammo") : PlayerPrefs.GetInt("upgrade_weapon_ammo", 1);
        int ammo_bonus = (ammo_level - 1) * 6;

        if (ranged_weapon != null)
        {
            ammo_in_mag = ranged_weapon.magazine_capacity;
            ammo_carried_max = ranged_weapon.carried_ammo_base + ammo_bonus;
            ammo_carried = ammo_carried_max;
        }

        active_weapon_type = GameManager.instance != null ? GameManager.instance.active_weapon_type : (WeaponType)PlayerPrefs.GetInt("active_weapon_type", (int)WeaponType.Ranged);
        UpdateWeaponVisual();
        NotifyConsumableChanged();
    }

    private void Update()
    {
        AimWeapon();
        HandleCombatInput();
    }

    private void AimWeapon()
    {
        if (weapon_mount == null || main_camera == null || Mouse.current == null) return;

        Vector3 mouse_screen = Mouse.current.position.ReadValue();
        Vector3 mouse_world = main_camera.ScreenToWorldPoint(mouse_screen);
        Vector2 aim_delta = mouse_world - weapon_mount.position;
        float aim_angle = Mathf.Atan2(aim_delta.y, aim_delta.x) * Mathf.Rad2Deg;
        weapon_mount.rotation = Quaternion.Euler(0f, 0f, aim_angle);
        weapon_mount.localScale = new Vector3(1f, aim_delta.x < 0f ? -1f : 1f, 1f);
    }

    private void HandleCombatInput()
    {
        Mouse mouse = Mouse.current;
        Keyboard kb = Keyboard.current;
        if (mouse == null || kb == null) return;

        bool is_combat = LevelManager.instance == null || LevelManager.instance.current_phase == GamePhase.Combat;

        // Consumable Slot Selection: Scroll wheel or numbers 1 & 2
        float scroll_y = mouse.scroll.ReadValue().y;
        if (scroll_y > 0.1f)
        {
            active_consumable_slot = 1;
            NotifyConsumableChanged();
        }
        else if (scroll_y < -0.1f)
        {
            active_consumable_slot = 2;
            NotifyConsumableChanged();
        }

        if (is_combat)
        {
            if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame)
            {
                active_consumable_slot = 1;
                NotifyConsumableChanged();
            }
            else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame)
            {
                active_consumable_slot = 2;
                NotifyConsumableChanged();
            }
        }

        // Use Consumable via Q
        if (kb.qKey.wasPressedThisFrame)
        {
            UseActiveConsumable();
        }

        if (!is_combat) return;

        // Left Click -> Auto-switch to Ranged and Shoot
        if (mouse.leftButton.isPressed)
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

        // Right Click -> Auto-switch to Melee and Strike
        if (mouse.rightButton.isPressed)
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

        if (kb.rKey.wasPressedThisFrame)
        {
            Reload();
        }
    }

    public void SetActiveWeaponType(WeaponType new_type)
    {
        if (active_weapon_type == new_type) return;
        active_weapon_type = new_type;
        if (GameManager.instance != null) GameManager.instance.active_weapon_type = active_weapon_type;
        PlayerPrefs.SetInt("active_weapon_type", (int)active_weapon_type);
        PlayerPrefs.Save();
        UpdateWeaponVisual();
    }

    public string GetActiveConsumableName()
    {
        if (active_consumable_slot == 1)
        {
            return GameManager.instance != null ? GameManager.instance.equipped_consumable_1 : PlayerPrefs.GetString("equipped_consumable_1", "Soap Bomb");
        }
        else
        {
            return GameManager.instance != null ? GameManager.instance.equipped_consumable_2 : PlayerPrefs.GetString("equipped_consumable_2", "First Aid Kit");
        }
    }

    public int GetActiveConsumableCount()
    {
        string item_name = GetActiveConsumableName();
        if (string.IsNullOrEmpty(item_name) || item_name == "None") return 0;
        return GameManager.instance != null ? GameManager.instance.GetConsumableCount(item_name) : PlayerPrefs.GetInt("consumable_" + item_name, 0);
    }

    public void UseActiveConsumable()
    {
        string item_name = GetActiveConsumableName();
        int count = GetActiveConsumableCount();
        if (string.IsNullOrEmpty(item_name) || item_name == "None" || count <= 0) return;

        bool success = ExecuteConsumableEffect(item_name);
        if (success)
        {
            int new_count = count - 1;
            if (GameManager.instance != null)
            {
                GameManager.instance.SetConsumableCount(item_name, new_count);
            }
            else
            {
                PlayerPrefs.SetInt("consumable_" + item_name, new_count);
                PlayerPrefs.Save();
            }
            NotifyConsumableChanged();
        }
    }

    private bool ExecuteConsumableEffect(string item_name)
    {
        Vector3 mouse_pos = main_camera != null && Mouse.current != null ? main_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) : transform.position;
        mouse_pos.z = 0f;

        if (item_name == "Soap Bomb")
        {
            Vector3 origin = fire_point != null ? fire_point.position : transform.position;
            GameObject prefab = soap_bomb_prefab;
            if (prefab == null) prefab = Resources.Load<GameObject>("Prefabs/SoapBomb");
            if (prefab != null)
            {
                GameObject bomb = Instantiate(prefab, origin, Quaternion.identity);
                SoapBombProjectile sbp = bomb.GetComponent<SoapBombProjectile>();
                Vector2 throw_dir = ((Vector2)(mouse_pos - origin)).normalized * 12f + Vector2.up * 4f;
                if (sbp != null) sbp.Launch(throw_dir);
                else
                {
                    Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = throw_dir;
                }
                return true;
            }
        }
        else if (item_name == "First Aid Kit")
        {
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.Heal(50);
                return true;
            }
        }
        else if (item_name == "Energy Drink")
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplySpeedBoost(1.5f, 8.0f);
                return true;
            }
        }
        else if (item_name == "Soap Water Bucket" || item_name == "Soap Bucket")
        {
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.shield_charges = 3;
                return true;
            }
        }
        else if (item_name == "Tools Box" || item_name == "Tool Box")
        {
            AddRepairPoints(50);
            if (Minimarket.instance != null && Minimarket.instance.health != null)
            {
                Minimarket.instance.health.Heal(50);
            }
            return true;
        }
        else if (item_name == "Sonar Batarang")
        {
            Vector3 origin = fire_point != null ? fire_point.position : transform.position;
            GameObject prefab = sonar_batarang_prefab;
            if (prefab == null) prefab = Resources.Load<GameObject>("Prefabs/SonarBatarang");
            if (prefab != null)
            {
                GameObject bat = Instantiate(prefab, origin, Quaternion.identity);
                SonarBatarangProjectile sbp = bat.GetComponent<SonarBatarangProjectile>();
                Vector2 fly_dir = ((Vector2)(mouse_pos - origin)).normalized;
                if (sbp != null) sbp.Launch(fly_dir);
                return true;
            }
        }

        return false;
    }

    public void NotifyConsumableChanged()
    {
        if (on_consumable_updated != null)
        {
            on_consumable_updated.Invoke(active_consumable_slot, GetActiveConsumableName(), GetActiveConsumableCount());
        }
    }

    private void ShootRanged()
    {
        if (ranged_weapon == null) return;
        if (ammo_in_mag <= 0)
        {
            Reload();
            return;
        }

        ammo_in_mag--;
        next_fire_time = Time.time + ranged_weapon.fire_rate;

        if (ranged_weapon.attack_sound != null)
        {
            AudioManager.PlaySFX(ranged_weapon.attack_sound);
        }

        Vector3 mouse_pos = main_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse_pos.z = 0f;
        Vector3 origin = fire_point != null ? fire_point.position : transform.position;
        Vector2 aim_dir = (mouse_pos - origin).normalized;
        float base_angle = Mathf.Atan2(aim_dir.y, aim_dir.x) * Mathf.Rad2Deg;

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

    private void PerformMelee()
    {
        if (melee_weapon == null) return;

        next_melee_time = Time.time + melee_weapon.attack_rate;

        if (melee_weapon.attack_sound != null)
        {
            AudioManager.PlaySFX(melee_weapon.attack_sound);
        }

        Vector3 origin = melee_point != null ? melee_point.position : transform.position;
        Collider2D[] hit_enemies = Physics2D.OverlapCircleAll(origin, melee_weapon.attack_range, enemy_layer);

        for (int i = 0; i < hit_enemies.Length; i++)
        {
            Health enemy_health = hit_enemies[i].GetComponent<Health>();
            if (enemy_health != null)
            {
                enemy_health.TakeDamage(melee_weapon.damage + weapon_damage_bonus);
            }
        }

        StartCoroutine(MeleeSwingCoroutine());
    }

    private System.Collections.IEnumerator MeleeSwingCoroutine()
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
        if (ranged_weapon == null) return;
        if (ammo_in_mag >= ranged_weapon.magazine_capacity || ammo_carried <= 0) return;

        int needed = ranged_weapon.magazine_capacity - ammo_in_mag;
        int take = Mathf.Min(needed, ammo_carried);
        ammo_in_mag += take;
        ammo_carried -= take;
    }

    public void Restock()
    {
        ammo_carried = ammo_carried_max;
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

            Transform fp = current_weapon_instance.transform.Find("FirePoint");
            if (fp != null) fire_point = fp;

            Transform mp = current_weapon_instance.transform.Find("MeleePoint");
            if (mp != null) melee_point = mp;
        }
    }
}
