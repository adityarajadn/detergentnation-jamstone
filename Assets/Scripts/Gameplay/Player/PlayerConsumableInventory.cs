using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerConsumableInventory : MonoBehaviour
{
    public int active_consumable_slot = 1;
    public GameObject soap_bomb_prefab;
    public GameObject sonar_batarang_prefab;

    [Header("Consumable Prefabs")]
    public GameObject[] consumable_prefabs;

    [Header("Events")]
    public UnityEvent<int, string, int> on_consumable_updated;

    [Header("Consumable Fallbacks")]
    [SerializeField] private int first_aid_heal = 50;
    [SerializeField] private float energy_drink_speed_boost = 1.5f;
    [SerializeField] private float energy_drink_duration = 8.0f;
    [SerializeField] private int soap_bucket_shields = 3;
    [SerializeField] private int tools_box_repair = 50;
    [SerializeField] private int tools_box_minimarket_heal = 50;
    [SerializeField] private Vector2 bomb_throw_impulse = new Vector2(12f, 4f);

    private Camera main_camera;
    private PlayerCombat combat;

    private void Awake()
    {
        main_camera = Camera.main;
        combat = GetComponent<PlayerCombat>();
    }

    private void Start()
    {
        NotifyConsumableChanged();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Mouse mouse = Mouse.current;
        Keyboard kb = Keyboard.current;
        if (mouse == null && kb == null) return;

        bool is_combat = LevelManager.instance == null || LevelManager.instance.current_phase == GamePhase.Combat;

        if (mouse != null)
        {
            float scroll_y = mouse.scroll.ReadValue().y;
            if (scroll_y > 0.1f)
            {
                SelectSlot(1);
            }
            else if (scroll_y < -0.1f)
            {
                SelectSlot(2);
            }
        }

        if (is_combat)
        {
            if (KeybindManager.WasPressedThisFrame(KeyAction.Slot1) || (kb != null && kb.numpad1Key.wasPressedThisFrame))
            {
                SelectSlot(1);
            }
            else if (KeybindManager.WasPressedThisFrame(KeyAction.Slot2) || (kb != null && kb.numpad2Key.wasPressedThisFrame))
            {
                SelectSlot(2);
            }
        }

        if (KeybindManager.WasPressedThisFrame(KeyAction.UseConsumable))
        {
            UseActiveConsumable();
        }
    }

    public void SelectSlot(int slot)
    {
        if (active_consumable_slot != slot)
        {
            active_consumable_slot = slot;
            NotifyConsumableChanged();
        }
    }

    public string GetActiveConsumableName()
    {
        if (active_consumable_slot == 1)
        {
            return GameManager.instance != null ? GameManager.instance.equipped_consumable_1 : GameConstants.EquipmentNames.DefaultConsumable1;
        }
        else
        {
            return GameManager.instance != null ? GameManager.instance.equipped_consumable_2 : GameConstants.EquipmentNames.DefaultConsumable2;
        }
    }

    public int GetActiveConsumableCount()
    {
        string item_name = GetActiveConsumableName();
        if (string.IsNullOrEmpty(item_name) || item_name == GameConstants.EquipmentNames.None) return 0;
        return GameManager.instance != null ? GameManager.instance.GetConsumableCount(item_name) : 0;
    }

    public void UseActiveConsumable()
    {
        string item_name = GetActiveConsumableName();
        int count = GetActiveConsumableCount();
        if (string.IsNullOrEmpty(item_name) || item_name == GameConstants.EquipmentNames.None || count <= 0) return;

        bool success = ExecuteConsumableEffect(item_name);
        if (success)
        {
            int new_count = count - 1;
            if (GameManager.instance != null)
            {
                GameManager.instance.SetConsumableCount(item_name, new_count);
            }
            NotifyConsumableChanged();
        }
    }

    private bool ExecuteConsumableEffect(string item_name)
    {
        if (main_camera == null) main_camera = Camera.main;
        Vector3 mouse_pos = main_camera != null && Mouse.current != null ? main_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) : transform.position;
        mouse_pos.z = 0f;

        if (consumable_prefabs != null)
        {
            for (int i = 0; i < consumable_prefabs.Length; i++)
            {
                if (consumable_prefabs[i] == null) continue;
                ConsumableBase cb = consumable_prefabs[i].GetComponent<ConsumableBase>();
                if (cb != null)
                {
                    bool match = cb.consumable_name.Equals(item_name, System.StringComparison.OrdinalIgnoreCase) ||
                        (cb.consumable_name == "Soap Water Bucket" && (item_name == "Soap Water Bucket" || item_name == "Soap Bucket")) ||
                        (cb.consumable_name == "Tools Box" && (item_name == "Tools Box" || item_name == "Tool Box"));
                    if (match)
                    {
                        return cb.Use(combat, mouse_pos);
                    }
                }
            }
        }

        Vector3 origin = combat != null && combat.fire_point != null ? combat.fire_point.position : transform.position;

        if (item_name == "Soap Bomb")
        {
            GameObject prefab = soap_bomb_prefab != null ? soap_bomb_prefab : (combat != null ? combat.soap_bomb_prefab : null);
            if (prefab != null)
            {
                GameObject bomb = Instantiate(prefab, origin, Quaternion.identity);
                SoapBombProjectile sbp = bomb.GetComponent<SoapBombProjectile>();
                Vector2 throw_dir = ((Vector2)(mouse_pos - origin)).normalized * bomb_throw_impulse.x + Vector2.up * bomb_throw_impulse.y;
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
                health.Heal(first_aid_heal);
                return true;
            }
        }
        else if (item_name == "Energy Drink")
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplySpeedBoost(energy_drink_speed_boost, energy_drink_duration);
                return true;
            }
        }
        else if (item_name == "Soap Water Bucket" || item_name == "Soap Bucket")
        {
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.shield_charges = soap_bucket_shields;
                return true;
            }
        }
        else if (item_name == "Tools Box" || item_name == "Tool Box")
        {
            if (combat != null) combat.AddRepairPoints(tools_box_repair);
            if (Minimarket.instance != null && Minimarket.instance.health != null)
            {
                Minimarket.instance.health.Heal(tools_box_minimarket_heal);
            }
            return true;
        }
        else if (item_name == "Sonar Batarang")
        {
            GameObject prefab = sonar_batarang_prefab != null ? sonar_batarang_prefab : (combat != null ? combat.sonar_batarang_prefab : null);
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
}
