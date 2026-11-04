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
