using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Buy / slot-assign logic and display for the consumables tab.
/// Plain C# helper owned by ShopUI (no scene component).
/// </summary>
public class ShopConsumableTab
{
    private readonly List<ConsumableShopEntry> entries;
    private readonly Action on_changed;

    public ShopConsumableTab(List<ConsumableShopEntry> entries, Action on_changed)
    {
        this.entries = entries;
        this.on_changed = on_changed;
    }

    public void Setup()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            int index = i;
            ConsumableShopEntry entry = entries[index];
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
        string prev1 = GameManager.instance.equipped_consumable_1;
        string prev2 = GameManager.instance.equipped_consumable_2;

        GameManager.instance.EquipConsumable(entry.item_name, slot);
        on_changed();

        string cur1 = GameManager.instance.equipped_consumable_1;
        string cur2 = GameManager.instance.equipped_consumable_2;

        if (slot == 1)
        {
            if (cur1 == entry.item_name)
            {
                if (prev2 == entry.item_name)
                    NotificationToast.Show("Moved " + entry.item_name + " to Slot 1!", ToastType.Info);
                else
                    NotificationToast.Show("Assigned " + entry.item_name + " to Slot 1!", ToastType.Success);
            }
            else
            {
                NotificationToast.Show("Unequipped " + entry.item_name + " from Slot 1!", ToastType.Warning);
            }
        }
        else
        {
            if (cur2 == entry.item_name)
            {
                if (prev1 == entry.item_name)
                    NotificationToast.Show("Moved " + entry.item_name + " to Slot 2!", ToastType.Info);
                else
                    NotificationToast.Show("Assigned " + entry.item_name + " to Slot 2!", ToastType.Success);
            }
            else
            {
                NotificationToast.Show("Unequipped " + entry.item_name + " from Slot 2!", ToastType.Warning);
            }
        }
    }

    private void OnBuyConsumable(ConsumableShopEntry entry)
    {
        int count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(entry.item_name) : PlayerPrefs.GetInt(GameConstants.PrefKeys.ConsumablePrefix + entry.item_name, 0);
        if (count >= 5)
        {
            NotificationToast.Show("Max capacity reached for " + entry.item_name + " (5/5)!", ToastType.Warning);
            return;
        }

        if (GameManager.instance != null && GameManager.instance.candy_total < entry.price)
        {
            NotificationToast.Show("Not enough candies to buy " + entry.item_name + "!", ToastType.Warning);
            return;
        }

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
            on_changed();
            NotificationToast.Show("Purchased " + entry.item_name + " (" + (count + 1) + "/5)!", ToastType.Success);
        }
    }

    public void Refresh()
    {
        string eq1 = GameManager.instance != null ? GameManager.instance.equipped_consumable_1 : "";
        string eq2 = GameManager.instance != null ? GameManager.instance.equipped_consumable_2 : "";

        for (int i = 0; i < entries.Count; i++)
        {
            ConsumableShopEntry entry = entries[i];
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
                entry.slot1_button_text.text = inSlot1 ? "SLOT 1 [ON]" : "SLOT 1";
            }

            if (entry.slot2_button_text != null)
            {
                entry.slot2_button_text.text = inSlot2 ? "SLOT 2 [ON]" : "SLOT 2";
            }
        }
    }
}
