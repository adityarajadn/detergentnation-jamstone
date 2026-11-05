using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>References to the loadout preview widgets owned by ShopUI.</summary>
public class ShopLoadoutPreview
{
    public TMP_Text ranged_text;
    public TMP_Text melee_text;
    public TMP_Text armor_text;
    public TMP_Text jetpack_text;
    public Image weapon_img;
    public Image melee_img;
    public Image armor_img;
    public Image jetpack_img;
}

/// <summary>
/// Buy / equip / stat-upgrade logic and display for the equipment tabs.
/// Plain C# helper owned by ShopUI (no scene component).
/// </summary>
public class ShopEquipmentTab
{
    private readonly List<EquipmentShopEntry> entries;
    private readonly ShopAccordion accordion;
    private readonly ShopLoadoutPreview preview;
    private readonly Action on_changed;

    public ShopEquipmentTab(List<EquipmentShopEntry> entries, ShopAccordion accordion, ShopLoadoutPreview preview, Action on_changed)
    {
        this.entries = entries;
        this.accordion = accordion;
        this.preview = preview;
        this.on_changed = on_changed;
    }

    public void Setup()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            int index = i;
            EquipmentShopEntry entry = entries[index];

            if (entry.row_expand_button != null)
            {
                entry.row_expand_button.onClick.AddListener(() => accordion.Toggle(entry));
            }

            if (entry.action_button != null)
            {
                entry.action_button.onClick.AddListener(() => OnEquipmentAction(entry));
            }

            if (entry.expand_panel != null)
            {
                entry.expand_panel.SetActive(false);
            }

            // Bind tooltip hover to row expand button / main row
            if (entry.row_expand_button != null)
            {
                UITooltipTrigger trigger = entry.row_expand_button.GetComponent<UITooltipTrigger>();
                if (trigger == null) trigger = entry.row_expand_button.gameObject.AddComponent<UITooltipTrigger>();
                trigger.SetTooltip(entry.item_name, entry.description);
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
            if (GameManager.instance != null && GameManager.instance.candy_total < entry.price)
            {
                NotificationToast.Show("Not enough candies to buy " + entry.item_name + "!", ToastType.Warning);
                return;
            }

            if (GameManager.instance != null && GameManager.instance.SpendCandy(entry.price))
            {
                GameManager.instance.SetEquipmentOwned(entry.item_name);
                EquipEntry(entry);
                on_changed();
                NotificationToast.Show("Purchased & equipped " + entry.item_name + "!", ToastType.Success);
            }
        }
        else
        {
            EquipEntry(entry);
            on_changed();
            NotificationToast.Show("Equipped " + entry.item_name + "!", ToastType.Success);
        }
    }

    private void OnStatUpgrade(EquipmentStatUpgrade stat_up)
    {
        if (stat_up == null) return;
        int current_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(stat_up.upgrade_id) : 1;
        if (current_level >= stat_up.max_level)
        {
            NotificationToast.Show(stat_up.stat_name + " is already at MAX level!", ToastType.Warning);
            return;
        }

        int cost = (current_level - 1) * stat_up.cost_multiplier;
        if (cost == 0) cost = stat_up.cost_multiplier;

        if (GameManager.instance != null && GameManager.instance.candy_total < cost)
        {
            NotificationToast.Show("Need " + cost + " candies to upgrade " + stat_up.stat_name + "!", ToastType.Warning);
            return;
        }

        if (GameManager.instance != null && GameManager.instance.SpendCandy(cost))
        {
            GameManager.instance.SetUpgradeLevel(stat_up.upgrade_id, current_level + 1);
            on_changed();
            NotificationToast.Show(stat_up.stat_name + " upgraded to Level " + (current_level + 1) + "!", ToastType.Success);
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
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].item_name == itemName)
            {
                return entries[i];
            }
        }
        return null;
    }

    public void Refresh()
    {
        string equipped_ranged = GameManager.instance != null ? GameManager.instance.equipped_ranged_weapon : GameConstants.EquipmentNames.DefaultRanged;
        string equipped_melee = GameManager.instance != null ? GameManager.instance.equipped_melee_weapon : GameConstants.EquipmentNames.DefaultMelee;
        string equipped_armor = GameManager.instance != null ? GameManager.instance.equipped_armor : GameConstants.EquipmentNames.None;
        string equipped_jetpack = GameManager.instance != null ? GameManager.instance.equipped_jetpack : GameConstants.EquipmentNames.None;

        for (int i = 0; i < entries.Count; i++)
        {
            EquipmentShopEntry entry = entries[i];
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

        if (preview.ranged_text != null) preview.ranged_text.text = "RANGED: " + equipped_ranged;
        if (preview.melee_text != null) preview.melee_text.text = "MELEE: " + equipped_melee;
        if (preview.armor_text != null) preview.armor_text.text = "ARMOR: " + equipped_armor;
        if (preview.jetpack_text != null) preview.jetpack_text.text = "JETPACK: " + equipped_jetpack;

        if (preview.armor_img != null)
        {
            preview.armor_img.enabled = equipped_armor != GameConstants.EquipmentNames.None;
            EquipmentShopEntry entry = FindEntry(equipped_armor);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview.armor_img.sprite = entry.preview_sprite;
            }
        }

        if (preview.jetpack_img != null)
        {
            preview.jetpack_img.enabled = equipped_jetpack != GameConstants.EquipmentNames.None;
            EquipmentShopEntry entry = FindEntry(equipped_jetpack);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview.jetpack_img.sprite = entry.preview_sprite;
            }
        }

        if (preview.weapon_img != null)
        {
            EquipmentShopEntry entry = FindEntry(equipped_ranged);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview.weapon_img.sprite = entry.preview_sprite;
            }
        }

        if (preview.melee_img != null)
        {
            preview.melee_img.enabled = !string.IsNullOrEmpty(equipped_melee) && equipped_melee != GameConstants.EquipmentNames.None;
            EquipmentShopEntry entry = FindEntry(equipped_melee);
            if (entry != null)
            {
                if (entry.preview_sprite != null) preview.melee_img.sprite = entry.preview_sprite;
            }
        }
    }

    public void RefreshStatUpgrades()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            EquipmentShopEntry entry = entries[i];
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
}
