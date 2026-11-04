using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop scene coordinator: header, category tabs, and wiring of the helper classes in UI/Shop/
/// (ShopAccordion, ShopEquipmentTab, ShopConsumableTab). Serialized fields stay here so scene
/// references are unchanged.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Top Header")]
    [SerializeField] private TMP_Text candy_balance_text;
    [SerializeField] private Button back_button;

    [Header("Category Tabs (5 Tabs)")]
    [SerializeField] private Button tab_melee_btn;
    [SerializeField] private Button tab_range_btn;
    [SerializeField] private Button tab_armor_btn;
    [SerializeField] private Button tab_jetpack_btn;
    [SerializeField] private Button tab_consume_btn;

    [Header("Category Panels")]
    [SerializeField] private GameObject melee_panel;
    [SerializeField] private GameObject range_panel;
    [SerializeField] private GameObject armor_panel;
    [SerializeField] private GameObject jetpack_panel;
    [SerializeField] private GameObject consume_panel;

    [Header("Player Preview & Loadout")]
    [SerializeField] private TMP_Text preview_ranged_text;
    [SerializeField] private TMP_Text preview_melee_text;
    [SerializeField] private TMP_Text preview_armor_text;
    [SerializeField] private TMP_Text preview_jetpack_text;
    [SerializeField] private Image preview_weapon_img;
    [SerializeField] private Image preview_melee_img;
    [SerializeField] private Image preview_armor_img;
    [SerializeField] private Image preview_jetpack_img;

    [Header("Entries")]
    public List<EquipmentShopEntry> equipment_entries = new List<EquipmentShopEntry>();
    public List<ConsumableShopEntry> consumable_entries = new List<ConsumableShopEntry>();

    [SerializeField] private float accordion_duration = 0.22f;

    private int active_tab = 0; // 0=Melee, 1=Range, 2=Armor, 3=Jetpack, 4=Consume
    private ShopAccordion accordion;
    private ShopEquipmentTab equipment_tab;
    private ShopConsumableTab consumable_tab;

    private void Start()
    {
        accordion = new ShopAccordion(this, equipment_entries, accordion_duration);
        equipment_tab = new ShopEquipmentTab(equipment_entries, accordion, BuildPreview(), RefreshAllDisplays);
        consumable_tab = new ShopConsumableTab(consumable_entries, RefreshAllDisplays);

        if (back_button != null)
        {
            back_button.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        if (tab_melee_btn != null) tab_melee_btn.onClick.AddListener(() => SwitchTab(0));
        if (tab_range_btn != null) tab_range_btn.onClick.AddListener(() => SwitchTab(1));
        if (tab_armor_btn != null) tab_armor_btn.onClick.AddListener(() => SwitchTab(2));
        if (tab_jetpack_btn != null) tab_jetpack_btn.onClick.AddListener(() => SwitchTab(3));
        if (tab_consume_btn != null) tab_consume_btn.onClick.AddListener(() => SwitchTab(4));

        equipment_tab.Setup();
        consumable_tab.Setup();

        SwitchTab(0);
        RefreshAllDisplays();
    }

    private ShopLoadoutPreview BuildPreview()
    {
        return new ShopLoadoutPreview
        {
            ranged_text = preview_ranged_text,
            melee_text = preview_melee_text,
            armor_text = preview_armor_text,
            jetpack_text = preview_jetpack_text,
            weapon_img = preview_weapon_img,
            melee_img = preview_melee_img,
            armor_img = preview_armor_img,
            jetpack_img = preview_jetpack_img
        };
    }

    public void SwitchTab(int tab_index)
    {
        active_tab = tab_index;
        if (melee_panel != null) melee_panel.SetActive(tab_index == 0);
        if (range_panel != null) range_panel.SetActive(tab_index == 1);
        if (armor_panel != null) armor_panel.SetActive(tab_index == 2);
        if (jetpack_panel != null) jetpack_panel.SetActive(tab_index == 3);
        if (consume_panel != null) consume_panel.SetActive(tab_index == 4);

        // Collapse all when switching tabs
        if (accordion != null) accordion.CollapseAll();
    }

    public void ToggleRowExpand(EquipmentShopEntry entry)
    {
        if (accordion != null) accordion.Toggle(entry);
    }

    public void RefreshAllDisplays()
    {
        if (candy_balance_text != null && GameManager.instance != null)
        {
            candy_balance_text.text = "Candies: " + GameManager.instance.candy_total;
        }

        if (equipment_tab != null) equipment_tab.Refresh();
        if (consumable_tab != null) consumable_tab.Refresh();
        if (equipment_tab != null) equipment_tab.RefreshStatUpgrades();
    }
}
