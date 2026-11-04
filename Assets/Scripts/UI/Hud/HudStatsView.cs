using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player / consumable / minimarket HUD readouts. Plain C# helper owned by GameplayUI;
/// widget references are assigned by GameplayUI from its serialized fields.
/// </summary>
public class HudStatsView
{
    // Player & combat
    public Slider player_health_slider;
    public TMP_Text ammo_text;
    public TMP_Text candy_text;
    public TMP_Text repair_points_text;

    // Consumables
    public GameObject consumable_panel;
    public TMP_Text consumable_slot1_text;
    public TMP_Text consumable_slot2_text;
    public Image consumable_slot1_icon;
    public Image consumable_slot2_icon;
    public Sprite icon_soap_bomb;
    public Sprite icon_first_aid;
    public Sprite icon_tools_box;
    public Sprite icon_energy_drink;
    public Sprite icon_soap_bucket;
    public Sprite icon_sonar_batarang;

    // Minimarket
    public Slider minimarket_health_slider;

    private PlayerCombat player_combat;
    private PlayerAmmo player_ammo;
    private Health player_health;

    public void Tick()
    {
        UpdatePlayerUI();
        UpdateMinimarketUI();
    }

    public void FindPlayerReferences()
    {
        if (PlayerCombat.Instance != null)
        {
            player_combat = PlayerCombat.Instance;
            player_ammo = PlayerCombat.Instance.GetComponent<PlayerAmmo>();
            player_health = PlayerCombat.Instance.GetComponent<Health>();
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
        if (player != null)
        {
            player_combat = player.GetComponent<PlayerCombat>();
            player_ammo = player.GetComponent<PlayerAmmo>();
            player_health = player.GetComponent<Health>();
        }
    }

    private void UpdatePlayerUI()
    {
        if (player_health == null || player_combat == null)
        {
            FindPlayerReferences();
            return;
        }

        if (player_health_slider != null)
        {
            player_health_slider.maxValue = player_health.health_max;
            player_health_slider.value = player_health.health_current;
        }

        if (ammo_text != null)
        {
            if (player_combat.active_weapon != null)
            {
                ammo_text.text = player_combat.active_weapon.GetAmmoDisplayText(player_ammo);
            }
            else if (player_combat.active_weapon_type == WeaponType.Melee)
            {
                ammo_text.text = player_combat.melee_weapon != null ? player_combat.melee_weapon.weapon_name : "MELEE";
            }
            else
            {
                int inMag = player_ammo != null ? player_ammo.ammo_in_mag : 0;
                int carried = player_ammo != null ? player_ammo.ammo_carried : 0;
                ammo_text.text = inMag + " / " + carried;
            }
        }

        if (candy_text != null && GameManager.instance != null)
        {
            candy_text.text = GameManager.instance.candy_total.ToString();
        }

        if (repair_points_text != null)
        {
            repair_points_text.text = player_combat.repair_points.ToString();
        }

        UpdateConsumableUI();
    }

    private void UpdateConsumableUI()
    {
        if (consumable_panel == null) return;

        string c1_name = GameManager.instance != null ? GameManager.instance.equipped_consumable_1 : GameConstants.EquipmentNames.DefaultConsumable1;
        string c2_name = GameManager.instance != null ? GameManager.instance.equipped_consumable_2 : GameConstants.EquipmentNames.DefaultConsumable2;

        int c1_count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(c1_name) : 0;
        int c2_count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(c2_name) : 0;

        if (consumable_slot1_text != null)
        {
            consumable_slot1_text.text = c1_count.ToString();
        }

        if (consumable_slot2_text != null)
        {
            consumable_slot2_text.text = c2_count.ToString();
        }

        if (consumable_slot1_icon != null)
        {
            Sprite s1 = GetConsumableSprite(c1_name);
            consumable_slot1_icon.sprite = s1;
            consumable_slot1_icon.enabled = s1 != null;
        }

        if (consumable_slot2_icon != null)
        {
            Sprite s2 = GetConsumableSprite(c2_name);
            consumable_slot2_icon.sprite = s2;
            consumable_slot2_icon.enabled = s2 != null;
        }

    }

    public Sprite GetConsumableSprite(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        switch (itemName)
        {
            case GameConstants.EquipmentNames.ConsumableSoapBomb: return icon_soap_bomb;
            case GameConstants.EquipmentNames.ConsumableFirstAidKit: return icon_first_aid;
            case GameConstants.EquipmentNames.ConsumableToolsBox: return icon_tools_box;
            case GameConstants.EquipmentNames.ConsumableEnergyDrink: return icon_energy_drink;
            case GameConstants.EquipmentNames.ConsumableSoapWaterBucket: return icon_soap_bucket;
            case GameConstants.EquipmentNames.ConsumableSonarBatarang: return icon_sonar_batarang;
        }
        return null;
    }

    private void UpdateMinimarketUI()
    {
        if (Minimarket.instance != null && Minimarket.instance.health != null && minimarket_health_slider != null)
        {
            minimarket_health_slider.maxValue = Minimarket.instance.health.health_max;
            minimarket_health_slider.value = Minimarket.instance.health.health_current;
        }
    }
}
