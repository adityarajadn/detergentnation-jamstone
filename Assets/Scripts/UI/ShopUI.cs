using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentCategory
{
    RangedWeapon,
    MeleeWeapon,
    Armor,
    Jetpack
}

[Serializable]
public class EquipmentShopEntry
{
    public string item_name;
    public EquipmentCategory category;
    public int price;
    public string description;
    public Sprite preview_sprite;
    public Color preview_color = Color.white;
    public GameObject item_prefab;
    public Button action_button;
    public TMP_Text action_button_text;
    public TMP_Text price_text;
}

[Serializable]
public class ConsumableShopEntry
{
    public string item_name;
    public int price;
    public string description;
    public Sprite preview_sprite;
    public GameObject item_prefab;
    public Button buy_button;
    public TMP_Text count_text;
    public TMP_Text price_text;
}

[Serializable]
public class UpgradeShopEntry
{
    public string upgrade_id;
    public string upgrade_title;
    public int max_level;
    public int cost_multiplier;
    public Button upgrade_button;
    public TMP_Text level_text;
    public TMP_Text cost_text;
}

public class ShopUI : MonoBehaviour
{
    [Header("Top Header")]
    [SerializeField] private TMP_Text candy_balance_text;
    [SerializeField] private Button back_button;

    [Header("Tab Navigation")]
    [SerializeField] private Button tab_equipment_btn;
    [SerializeField] private Button tab_consumable_btn;
    [SerializeField] private Button tab_upgrades_btn;

    [Header("Tab Panels")]
    [SerializeField] private GameObject equipment_panel;
    [SerializeField] private GameObject consumable_panel;
    [SerializeField] private GameObject upgrades_panel;

    [Header("Player Preview & Loadout")]
    [SerializeField] private TMP_Text preview_ranged_text;
    [SerializeField] private TMP_Text preview_melee_text;
    [SerializeField] private TMP_Text preview_armor_text;
    [SerializeField] private TMP_Text preview_jetpack_text;
    [SerializeField] private Image preview_weapon_img;
    [SerializeField] private Image preview_armor_img;
    [SerializeField] private Image preview_jetpack_img;

    [Header("Entries")]
    public List<EquipmentShopEntry> equipment_entries = new List<EquipmentShopEntry>();
    public List<ConsumableShopEntry> consumable_entries = new List<ConsumableShopEntry>();
    public List<UpgradeShopEntry> upgrade_entries = new List<UpgradeShopEntry>();

    private void Start()
    {
        if (back_button != null)
        {
            back_button.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        if (tab_equipment_btn != null)
        {
            tab_equipment_btn.onClick.AddListener(() => SwitchTab(0));
        }

        if (tab_consumable_btn != null)
        {
            tab_consumable_btn.onClick.AddListener(() => SwitchTab(1));
        }

        if (tab_upgrades_btn != null)
        {
            tab_upgrades_btn.onClick.AddListener(() => SwitchTab(2));
        }

        SetupEquipmentButtons();
        SetupConsumableButtons();
        SetupUpgradeButtons();

        SwitchTab(0);
        RefreshAllDisplays();
    }

    private void SwitchTab(int tab_index)
    {
        if (equipment_panel != null) equipment_panel.SetActive(tab_index == 0);
        if (consumable_panel != null) consumable_panel.SetActive(tab_index == 1);
        if (upgrades_panel != null) upgrades_panel.SetActive(tab_index == 2);
    }

    public void RefreshAllDisplays()
    {
        if (candy_balance_text != null && GameManager.instance != null)
        {
            candy_balance_text.text = "Candies: " + GameManager.instance.candy_total;
        }

        RefreshEquipmentDisplay();
        RefreshConsumableDisplay();
        RefreshUpgradeDisplay();
    }

    private void SetupEquipmentButtons()
    {
        for (int i = 0; i < equipment_entries.Count; i++)
        {
            int index = i;
            EquipmentShopEntry entry = equipment_entries[index];
            if (entry.action_button != null)
            {
                entry.action_button.onClick.AddListener(() => OnEquipmentAction(entry));
            }
        }
    }

    private void OnEquipmentAction(EquipmentShopEntry entry)
    {
        bool is_owned = GameManager.instance != null && GameManager.instance.IsEquipmentOwned(entry.item_name, entry.price);

        if (!is_owned)
        {
            if (GameManager.instance != null && GameManager.instance.SpendCandy(entry.price))
            {
                GameManager.instance.SetEquipmentOwned(entry.item_name);
                EquipEntry(entry);
                RefreshAllDisplays();
            }
        }
        else
        {
            EquipEntry(entry);
            RefreshAllDisplays();
        }
    }

    private void EquipEntry(EquipmentShopEntry entry)
    {
        if (GameManager.instance == null) return;
        switch (entry.category)
        {
            case EquipmentCategory.RangedWeapon:
                GameManager.instance.EquipWeapon(entry.item_name, true);
                break;
            case EquipmentCategory.MeleeWeapon:
                GameManager.instance.EquipWeapon(entry.item_name, false);
                break;
            case EquipmentCategory.Armor:
                GameManager.instance.EquipArmor(entry.item_name);
                break;
            case EquipmentCategory.Jetpack:
                GameManager.instance.EquipJetpack(entry.item_name);
                break;
        }
    }

    private EquipmentShopEntry FindEntry(string itemName)
    {
        if (equipment_entries == null || string.IsNullOrEmpty(itemName)) return null;
        for (int i = 0; i < equipment_entries.Count; i++)
        {
            if (equipment_entries[i] != null && equipment_entries[i].item_name == itemName)
                return equipment_entries[i];
        }
        return null;
    }

    private Color GetEntryPreviewColor(EquipmentShopEntry entry)
    {
        if (entry == null) return Color.white;
        if (entry.preview_color != Color.white) return entry.preview_color;

        switch (entry.item_name)
        {
            case "Worker's Hat": return new Color(0.95f, 0.75f, 0.1f);
            case "I am Number One": return new Color(0.2f, 0.6f, 0.95f);
            case "Octosoap": return new Color(0.95f, 0.35f, 0.5f);
            case "Joyride": return new Color(0.3f, 0.85f, 0.95f);
            case "I am Vengeance": return new Color(0.2f, 0.15f, 0.25f);
            case "Water Pistol": return new Color(0.3f, 0.8f, 1f);
            case "Water Rifle": return new Color(0.2f, 0.5f, 0.9f);
            case "Water Plasma": return new Color(0.9f, 0.4f, 1f);
            case "Bubble Gun": return new Color(0.4f, 0.95f, 0.7f);
            default: return entry.preview_color;
        }
    }

    private void RefreshEquipmentDisplay()
    {
        string equipped_ranged = GameManager.instance != null ? GameManager.instance.equipped_ranged_weapon : GameConstants.EquipmentNames.DefaultRanged;
        string equipped_melee = GameManager.instance != null ? GameManager.instance.equipped_melee_weapon : GameConstants.EquipmentNames.DefaultMelee;
        string equipped_armor = GameManager.instance != null ? GameManager.instance.equipped_armor : GameConstants.EquipmentNames.None;
        string equipped_jetpack = GameManager.instance != null ? GameManager.instance.equipped_jetpack : GameConstants.EquipmentNames.None;

        for (int i = 0; i < equipment_entries.Count; i++)
        {
            EquipmentShopEntry entry = equipment_entries[i];
            bool is_owned = GameManager.instance != null && GameManager.instance.IsEquipmentOwned(entry.item_name, entry.price);

            bool is_equipped = false;
            switch (entry.category)
            {
                case EquipmentCategory.RangedWeapon:
                    is_equipped = (equipped_ranged == entry.item_name);
                    break;
                case EquipmentCategory.MeleeWeapon:
                    is_equipped = (equipped_melee == entry.item_name);
                    break;
                case EquipmentCategory.Armor:
                    is_equipped = (equipped_armor == entry.item_name);
                    break;
                case EquipmentCategory.Jetpack:
                    is_equipped = (equipped_jetpack == entry.item_name);
                    break;
            }

            if (entry.price_text != null)
            {
                entry.price_text.text = entry.price == 0 ? "Default" : entry.price + " Candy";
            }

            if (entry.action_button_text != null)
            {
                if (is_equipped)
                {
                    entry.action_button_text.text = "EQUIPPED";
                }
                else if (is_owned)
                {
                    entry.action_button_text.text = "EQUIP";
                }
                else
                {
                    entry.action_button_text.text = "BUY (" + entry.price + " C)";
                }
            }

            if (entry.action_button != null)
            {
                entry.action_button.interactable = !is_equipped;
            }
        }

        if (preview_ranged_text != null) preview_ranged_text.text = "RANGED: " + equipped_ranged;
        if (preview_melee_text != null) preview_melee_text.text = "MELEE: " + equipped_melee;
        if (preview_armor_text != null) preview_armor_text.text = "ARMOR: " + equipped_armor;
        if (preview_jetpack_text != null) preview_jetpack_text.text = "JETPACK: " + equipped_jetpack;

        if (preview_armor_img != null)
        {
            preview_armor_img.enabled = equipped_armor != GameConstants.EquipmentNames.None;
            EquipmentShopEntry entry = FindEntry(equipped_armor);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview_armor_img.sprite = entry.preview_sprite;
                preview_armor_img.color = GetEntryPreviewColor(entry);
            }
        }

        if (preview_jetpack_img != null)
        {
            preview_jetpack_img.enabled = equipped_jetpack != GameConstants.EquipmentNames.None;
            EquipmentShopEntry entry = FindEntry(equipped_jetpack);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview_jetpack_img.sprite = entry.preview_sprite;
                preview_jetpack_img.color = GetEntryPreviewColor(entry);
            }
        }

        if (preview_weapon_img != null)
        {
            EquipmentShopEntry entry = FindEntry(equipped_ranged);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview_weapon_img.sprite = entry.preview_sprite;
                preview_weapon_img.color = GetEntryPreviewColor(entry);
            }
        }
    }

    private void SetupConsumableButtons()
    {
        for (int i = 0; i < consumable_entries.Count; i++)
        {
            int index = i;
            ConsumableShopEntry entry = consumable_entries[index];
            if (entry.buy_button != null)
            {
                entry.buy_button.onClick.AddListener(() => OnBuyConsumable(entry));
            }
        }
    }

    private void OnBuyConsumable(ConsumableShopEntry entry)
    {
        int count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(entry.item_name) : PlayerPrefs.GetInt("consumable_" + entry.item_name, 0);
        if (count >= 3) return;

        if (GameManager.instance != null && GameManager.instance.SpendCandy(entry.price))
        {
            GameManager.instance.SetConsumableCount(entry.item_name, count + 1);
            if (GameManager.instance.equipped_consumable_1 != entry.item_name && GameManager.instance.equipped_consumable_2 != entry.item_name)
            {
                int c1 = GameManager.instance.GetConsumableCount(GameManager.instance.equipped_consumable_1);
                int c2 = GameManager.instance.GetConsumableCount(GameManager.instance.equipped_consumable_2);
                if (c1 <= 0)
                {
                    GameManager.instance.EquipConsumable(entry.item_name, 1);
                }
                else if (c2 <= 0)
                {
                    GameManager.instance.EquipConsumable(entry.item_name, 2);
                }
            }
            RefreshAllDisplays();
        }
    }

    private void RefreshConsumableDisplay()
    {
        for (int i = 0; i < consumable_entries.Count; i++)
        {
            ConsumableShopEntry entry = consumable_entries[i];
            int count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(entry.item_name) : 0;

            if (entry.count_text != null)
            {
                entry.count_text.text = count + " / 3";
            }

            if (entry.buy_button != null)
            {
                entry.buy_button.interactable = count < 3;
            }
        }
    }

    private void SetupUpgradeButtons()
    {
        for (int i = 0; i < upgrade_entries.Count; i++)
        {
            int index = i;
            UpgradeShopEntry entry = upgrade_entries[index];
            if (entry.upgrade_button != null)
            {
                entry.upgrade_button.onClick.AddListener(() => OnUpgradeAction(entry));
            }
        }
    }

    private void OnUpgradeAction(UpgradeShopEntry entry)
    {
        int current_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(entry.upgrade_id) : 1;
        if (current_level >= entry.max_level) return;

        int cost = (current_level - 1) * entry.cost_multiplier;
        if (cost == 0) cost = entry.cost_multiplier;

        if (GameManager.instance != null && GameManager.instance.SpendCandy(cost))
        {
            GameManager.instance.SetUpgradeLevel(entry.upgrade_id, current_level + 1);
            RefreshAllDisplays();
        }
    }

    private void RefreshUpgradeDisplay()
    {
        for (int i = 0; i < upgrade_entries.Count; i++)
        {
            UpgradeShopEntry entry = upgrade_entries[i];
            int current_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(entry.upgrade_id) : 1;

            if (entry.level_text != null)
            {
                entry.level_text.text = "Level " + current_level + " / " + entry.max_level;
            }

            if (entry.cost_text != null)
            {
                if (current_level >= entry.max_level)
                {
                    entry.cost_text.text = "MAX LEVEL";
                }
                else
                {
                    int cost = (current_level - 1) * entry.cost_multiplier;
                    if (cost == 0) cost = entry.cost_multiplier;
                    entry.cost_text.text = cost + " Candy";
                }
            }

            if (entry.upgrade_button != null)
            {
                entry.upgrade_button.interactable = current_level < entry.max_level;
            }
        }
    }
}
