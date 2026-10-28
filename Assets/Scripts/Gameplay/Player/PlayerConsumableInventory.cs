using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerConsumableInventory : MonoBehaviour
{
    public int active_consumable_slot = 1;

    [Header("Consumable Prefabs")]
    public GameObject[] consumable_prefabs;

    [Header("Events")]
    public UnityEvent<int, string, int> on_consumable_updated;

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
        if (Time.timeScale == 0f) return;
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

        if (KeybindManager.WasPressedThisFrame(KeyAction.Slot1) || (kb != null && kb.numpad1Key.wasPressedThisFrame))
        {
            SelectSlot(1);
        }
        else if (KeybindManager.WasPressedThisFrame(KeyAction.Slot2) || (kb != null && kb.numpad2Key.wasPressedThisFrame))
        {
            SelectSlot(2);
        }

        if (is_combat && KeybindManager.WasPressedThisFrame(KeyAction.UseConsumable))
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
        if (string.IsNullOrEmpty(item_name) || item_name == GameConstants.EquipmentNames.None)
        {
            NotificationToast.Show("No consumable equipped in Slot " + active_consumable_slot + "!", ToastType.Warning);
            return;
        }
        if (count <= 0)
        {
            NotificationToast.Show("No " + item_name + " remaining!", ToastType.Warning);
            return;
        }

        bool success = ExecuteConsumableEffect(item_name);
        if (success)
        {
            ShowConsumablePopup(item_name);
            int new_count = count - 1;
            if (GameManager.instance != null)
            {
                GameManager.instance.SetConsumableCount(item_name, new_count);
            }
            NotifyConsumableChanged();
        }
    }

    private void ShowConsumablePopup(string item_name)
    {
        string label;
        Color color;

        if (item_name.IndexOf("First Aid", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            label = "+50 HP";
            color = new Color(0.29f, 0.87f, 0.5f, 1f); // Green
        }
        else if (item_name.IndexOf("Energy Drink", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            label = "SPEED UP!";
            color = new Color(0.22f, 0.74f, 0.97f, 1f); // Blue
        }
        else if (item_name.IndexOf("Tool", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            label = "+REPAIR";
            color = new Color(0.98f, 0.57f, 0.24f, 1f); // Orange
        }
        else if (item_name.IndexOf("Soap Bomb", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            label = "SOAP BOMB!";
            color = new Color(1f, 0.88f, 0.32f, 1f); // Yellow
        }
        else if (item_name.IndexOf("Bucket", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            label = "SOAP BUCKET!";
            color = new Color(0.4f, 0.9f, 1f, 1f); // Cyan
        }
        else if (item_name.IndexOf("Sonar", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            label = "SONAR!";
            color = new Color(0.75f, 0.55f, 1f, 1f); // Purple
        }
        else
        {
            label = item_name.ToUpper();
            color = new Color(1f, 0.88f, 0.32f, 1f);
        }

        DamagePopup.SpawnText(transform.position + Vector3.up * 0.8f, label, color);
    }

    private bool ExecuteConsumableEffect(string item_name)
    {
        if (consumable_prefabs == null) return false;

        if (main_camera == null) main_camera = Camera.main;
        Vector3 mouse_pos = main_camera != null && Mouse.current != null ? main_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) : transform.position;
        mouse_pos.z = 0f;

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
