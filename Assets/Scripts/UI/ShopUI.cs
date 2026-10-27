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
public class EquipmentStatUpgrade
{
    public string upgrade_id;
    public string stat_name;
    public int max_level = 10;
    public int cost_multiplier = 30;
    public Slider level_slider;
    public Button upgrade_button;
    public TMP_Text cost_text;
    public TMP_Text level_text;
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

    [Header("Row Elements")]
    public Button row_expand_button; // clicking row expands/collapses
    public GameObject expand_panel;   // collapsible sub-panel
    public Image item_icon_img;
    public TMP_Text name_text;
    public TMP_Text price_text;
    public Button action_button;      // Buy or Equip
    public TMP_Text action_button_text;

    [Header("Inline Upgrades")]
    public List<EquipmentStatUpgrade> stat_upgrades = new List<EquipmentStatUpgrade>();
}

[Serializable]
public class ConsumableShopEntry
{
    public string item_name;
    public int price;
    public string description;
    public Sprite preview_sprite;
    public GameObject item_prefab;

    [Header("UI Row References")]
    public Image item_icon_img;
    public TMP_Text name_text;
    public TMP_Text desc_text;
    public TMP_Text count_text;
    public Button buy_button;
    public TMP_Text price_text;
    public Button slot1_button;
    public TMP_Text slot1_button_text;
    public Button slot2_button;
    public TMP_Text slot2_button_text;
}

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

    private int active_tab = 0; // 0=Melee, 1=Range, 2=Armor, 3=Jetpack, 4=Consume
    private EquipmentShopEntry currently_expanded_entry = null;
    private Coroutine active_accordion_routine = null;
    [SerializeField] private float accordion_duration = 0.22f;

    private void Start()
    {
        if (back_button != null)
        {
            back_button.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        if (tab_melee_btn != null) tab_melee_btn.onClick.AddListener(() => SwitchTab(0));
        if (tab_range_btn != null) tab_range_btn.onClick.AddListener(() => SwitchTab(1));
        if (tab_armor_btn != null) tab_armor_btn.onClick.AddListener(() => SwitchTab(2));
        if (tab_jetpack_btn != null) tab_jetpack_btn.onClick.AddListener(() => SwitchTab(3));
        if (tab_consume_btn != null) tab_consume_btn.onClick.AddListener(() => SwitchTab(4));

        SetupEquipmentRows();
        SetupConsumableButtons();

        SwitchTab(0);
        RefreshAllDisplays();
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
        CollapseAllRows();
    }

    private void CollapseAllRows()
    {
        if (active_accordion_routine != null)
        {
            StopCoroutine(active_accordion_routine);
            active_accordion_routine = null;
        }

        for (int i = 0; i < equipment_entries.Count; i++)
        {
            if (equipment_entries[i].expand_panel != null)
            {
                equipment_entries[i].expand_panel.SetActive(false);
                LayoutElement le = equipment_entries[i].expand_panel.GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredHeight = 0;
                }
            }
        }
        currently_expanded_entry = null;
    }

    public void ToggleRowExpand(EquipmentShopEntry entry)
    {
        if (entry == null || entry.expand_panel == null) return;

        bool will_expand = (currently_expanded_entry != entry);

        if (active_accordion_routine != null)
        {
            StopCoroutine(active_accordion_routine);
            active_accordion_routine = null;
        }

        active_accordion_routine = StartCoroutine(AnimateAccordionRoutine(entry, will_expand));
    }

    private System.Collections.IEnumerator AnimateAccordionRoutine(EquipmentShopEntry targetEntry, bool will_expand)
    {
        EquipmentShopEntry closingEntry = currently_expanded_entry;
        EquipmentShopEntry openingEntry = will_expand ? targetEntry : null;

        // Calculate target height for opening entry
        float openingTargetHeight = 0f;
        LayoutElement openingLE = null;
        CanvasGroup openingCG = null;
        RectTransform openingRT = null;

        if (openingEntry != null && openingEntry.expand_panel != null)
        {
            openingEntry.expand_panel.SetActive(true);
            openingLE = openingEntry.expand_panel.GetComponent<LayoutElement>();
            openingCG = openingEntry.expand_panel.GetComponent<CanvasGroup>();
            openingRT = openingEntry.expand_panel.GetComponent<RectTransform>();

            // Calculate height by forcing layout calculation
            if (openingLE != null) openingLE.preferredHeight = -1; // unrestricted for measurement
            LayoutRebuilder.ForceRebuildLayoutImmediate(openingRT);
            openingTargetHeight = LayoutUtility.GetPreferredHeight(openingRT);
            if (openingTargetHeight <= 0f) openingTargetHeight = 65f * Mathf.Max(1, openingEntry.stat_upgrades.Count);
            if (openingLE != null) openingLE.preferredHeight = 0f;
            if (openingCG != null) openingCG.alpha = 0f;
        }

        // Setup closing entry
        float closingStartHeight = 0f;
        LayoutElement closingLE = null;
        CanvasGroup closingCG = null;

        if (closingEntry != null && closingEntry != openingEntry && closingEntry.expand_panel != null && closingEntry.expand_panel.activeSelf)
        {
            closingLE = closingEntry.expand_panel.GetComponent<LayoutElement>();
            closingCG = closingEntry.expand_panel.GetComponent<CanvasGroup>();
            closingStartHeight = closingLE != null && closingLE.preferredHeight > 0 ? closingLE.preferredHeight : LayoutUtility.GetPreferredHeight(closingEntry.expand_panel.GetComponent<RectTransform>());
        }

        currently_expanded_entry = openingEntry;

        Transform activePanelContent = targetEntry.expand_panel.transform.parent != null ? targetEntry.expand_panel.transform.parent.parent : null;
        RectTransform contentRT = activePanelContent != null ? activePanelContent.GetComponent<RectTransform>() : null;

        float elapsed = 0f;
        while (elapsed < accordion_duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / accordion_duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (openingLE != null)
            {
                openingLE.preferredHeight = Mathf.Lerp(0f, openingTargetHeight, smoothT);
                if (openingCG != null) openingCG.alpha = smoothT;
            }

            if (closingLE != null)
            {
                closingLE.preferredHeight = Mathf.Lerp(closingStartHeight, 0f, smoothT);
                if (closingCG != null) closingCG.alpha = 1f - smoothT;
            }

            if (contentRT != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(contentRT);
            }

            yield return null;
        }

        // Finalize state
        if (openingLE != null)
        {
            openingLE.preferredHeight = openingTargetHeight;
            if (openingCG != null) openingCG.alpha = 1f;
        }

        if (closingLE != null && closingEntry != null && closingEntry.expand_panel != null)
        {
            closingLE.preferredHeight = 0f;
            if (closingCG != null) closingCG.alpha = 0f;
            closingEntry.expand_panel.SetActive(false);
        }

        if (contentRT != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
        }

        active_accordion_routine = null;
    }

    public void RefreshAllDisplays()
    {
        if (candy_balance_text != null && GameManager.instance != null)
        {
            candy_balance_text.text = "Candies: " + GameManager.instance.candy_total;
        }

        RefreshEquipmentDisplay();
        RefreshConsumableDisplay();
        RefreshStatUpgrades();
    }

    private void SetupEquipmentRows()
    {
        for (int i = 0; i < equipment_entries.Count; i++)
        {
            int index = i;
            EquipmentShopEntry entry = equipment_entries[index];

            if (entry.row_expand_button != null)
            {
                entry.row_expand_button.onClick.AddListener(() => ToggleRowExpand(entry));
            }

            if (entry.action_button != null)
            {
                entry.action_button.onClick.AddListener(() => OnEquipmentAction(entry));
            }

            if (entry.expand_panel != null)
            {
                entry.expand_panel.SetActive(false);
            }

            // Bind inline stat upgrade buttons
            if (entry.stat_upgrades != null)
            {
                for (int u = 0; u < entry.stat_upgrades.Count; u++)
                {
                    EquipmentStatUpgrade stat_up = entry.stat_upgrades[u];
                    if (stat_up.upgrade_button != null)
                    {
                        stat_up.upgrade_button.onClick.AddListener(() => OnStatUpgrade(stat_up));
                    }
                }
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

    private void OnStatUpgrade(EquipmentStatUpgrade stat_up)
    {
        if (stat_up == null) return;
        int current_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(stat_up.upgrade_id) : 1;
        if (current_level >= stat_up.max_level) return;

        int cost = (current_level - 1) * stat_up.cost_multiplier;
        if (cost == 0) cost = stat_up.cost_multiplier;

        if (GameManager.instance != null && GameManager.instance.SpendCandy(cost))
        {
            GameManager.instance.SetUpgradeLevel(stat_up.upgrade_id, current_level + 1);
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
        for (int i = 0; i < equipment_entries.Count; i++)
        {
            if (equipment_entries[i].item_name == itemName)
            {
                return equipment_entries[i];
            }
        }
        return null;
    }

    private Color GetEntryPreviewColor(EquipmentShopEntry entry)
    {
        if (entry != null && entry.preview_color != Color.clear && entry.preview_color.a > 0f)
        {
            return entry.preview_color;
        }
        return Color.white;
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
                    is_equipped = equipped_ranged == entry.item_name;
                    break;
                case EquipmentCategory.MeleeWeapon:
                    is_equipped = equipped_melee == entry.item_name;
                    break;
                case EquipmentCategory.Armor:
                    is_equipped = equipped_armor == entry.item_name;
                    break;
                case EquipmentCategory.Jetpack:
                    is_equipped = equipped_jetpack == entry.item_name;
                    break;
            }

            if (entry.name_text != null)
            {
                entry.name_text.text = entry.item_name;
            }

            if (entry.item_icon_img != null && entry.preview_sprite != null)
            {
                entry.item_icon_img.sprite = entry.preview_sprite;
                entry.item_icon_img.color = GetEntryPreviewColor(entry);
            }

            if (entry.price_text != null)
            {
                if (is_owned)
                {
                    entry.price_text.gameObject.SetActive(false);
                }
                else
                {
                    entry.price_text.gameObject.SetActive(true);
                    entry.price_text.text = entry.price == 0 ? "Free" : entry.price + " Candy";
                }
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
                    entry.action_button_text.text = "BUY";
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

        if (preview_melee_img != null)
        {
            preview_melee_img.enabled = !string.IsNullOrEmpty(equipped_melee) && equipped_melee != GameConstants.EquipmentNames.None;
            EquipmentShopEntry entry = FindEntry(equipped_melee);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview_melee_img.sprite = entry.preview_sprite;
                preview_melee_img.color = GetEntryPreviewColor(entry);
            }
        }
    }

    private void RefreshStatUpgrades()
    {
        for (int i = 0; i < equipment_entries.Count; i++)
        {
            EquipmentShopEntry entry = equipment_entries[i];
            if (entry.stat_upgrades == null) continue;

            for (int u = 0; u < entry.stat_upgrades.Count; u++)
            {
                EquipmentStatUpgrade stat_up = entry.stat_upgrades[u];
                int current_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(stat_up.upgrade_id) : 1;

                if (stat_up.level_slider != null)
                {
                    stat_up.level_slider.minValue = 1;
                    stat_up.level_slider.maxValue = stat_up.max_level;
                    stat_up.level_slider.value = current_level;
                }

                if (stat_up.level_text != null)
                {
                    stat_up.level_text.text = current_level + " / " + stat_up.max_level;
                }

                if (stat_up.cost_text != null)
                {
                    if (current_level >= stat_up.max_level)
                    {
                        stat_up.cost_text.text = "MAX";
                    }
                    else
                    {
                        int cost = (current_level - 1) * stat_up.cost_multiplier;
                        if (cost == 0) cost = stat_up.cost_multiplier;
                        stat_up.cost_text.text = "+" + cost + " C";
                    }
                }

                if (stat_up.upgrade_button != null)
                {
                    stat_up.upgrade_button.interactable = current_level < stat_up.max_level;
                }
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
            if (entry.slot1_button != null)
            {
                entry.slot1_button.onClick.AddListener(() => OnEquipConsumableSlot(entry, 1));
            }
            if (entry.slot2_button != null)
            {
                entry.slot2_button.onClick.AddListener(() => OnEquipConsumableSlot(entry, 2));
            }
        }
    }

    private void OnEquipConsumableSlot(ConsumableShopEntry entry, int slot)
    {
        if (GameManager.instance == null) return;
        GameManager.instance.EquipConsumable(entry.item_name, slot);
        RefreshAllDisplays();
    }

    private void OnBuyConsumable(ConsumableShopEntry entry)
    {
        int count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(entry.item_name) : PlayerPrefs.GetInt("consumable_" + entry.item_name, 0);
        if (count >= 5) return;

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
        string eq1 = GameManager.instance != null ? GameManager.instance.equipped_consumable_1 : "";
        string eq2 = GameManager.instance != null ? GameManager.instance.equipped_consumable_2 : "";

        Color activeSlotColor = new Color(0.20f, 0.65f, 0.45f, 1f); // Green
        Color inactiveSlotColor = new Color(0.22f, 0.25f, 0.36f, 1f); // Dark Grey/Blue

        for (int i = 0; i < consumable_entries.Count; i++)
        {
            ConsumableShopEntry entry = consumable_entries[i];
            int count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(entry.item_name) : 0;

            if (entry.name_text != null) entry.name_text.text = entry.item_name;
            if (entry.desc_text != null) entry.desc_text.text = entry.description;

            if (entry.item_icon_img != null && entry.preview_sprite != null)
            {
                entry.item_icon_img.sprite = entry.preview_sprite;
            }

            if (entry.count_text != null)
            {
                entry.count_text.text = count + " / 5";
            }

            if (entry.price_text != null)
            {
                entry.price_text.text = "+ " + entry.price + " C";
            }

            if (entry.buy_button != null)
            {
                entry.buy_button.interactable = count < 5;
            }

            bool inSlot1 = eq1 == entry.item_name;
            bool inSlot2 = eq2 == entry.item_name;

            if (entry.slot1_button_text != null)
            {
                entry.slot1_button_text.text = inSlot1 ? "SLOT 1 [ACTIVE]" : "SLOT 1";
            }
            if (entry.slot1_button != null)
            {
                Image s1Img = entry.slot1_button.GetComponent<Image>();
                if (s1Img != null) s1Img.color = inSlot1 ? activeSlotColor : inactiveSlotColor;
            }

            if (entry.slot2_button_text != null)
            {
                entry.slot2_button_text.text = inSlot2 ? "SLOT 2 [ACTIVE]" : "SLOT 2";
            }
            if (entry.slot2_button != null)
            {
                Image s2Img = entry.slot2_button.GetComponent<Image>();
                if (s2Img != null) s2Img.color = inSlot2 ? activeSlotColor : inactiveSlotColor;
            }
        }
    }
}
