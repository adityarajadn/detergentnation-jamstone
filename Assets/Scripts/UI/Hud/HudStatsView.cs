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
    public Image player_health_fill_image;
    public Sprite playerbar_green;
    public Sprite playerbar_yellow;
    public Sprite playerbar_red;

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

    private PlayerCombat player_combat;
    private PlayerAmmo player_ammo;
    private Health player_health;

    private int last_hp = -1;
    private int last_max_hp = -1;
    private int last_candy = -1;
    private int last_repair = -1;
    private string last_ammo_str = null;
    private int last_c1_count = -1;
    private int last_c2_count = -1;
    private string last_c1_name = null;
    private string last_c2_name = null;

    public void Tick()
    {
        UpdatePlayerUI();
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

        if (player_health_slider != null && (player_health.health_current != last_hp || player_health.health_max != last_max_hp))
        {
            last_hp = player_health.health_current;
            last_max_hp = player_health.health_max;
            player_health_slider.maxValue = last_max_hp;
            player_health_slider.value = last_hp;

            if (player_health_fill_image != null && last_max_hp > 0)
            {
                float ratio = (float)last_hp / last_max_hp;
                if (ratio > 0.6f && playerbar_green != null)
                {
                    player_health_fill_image.sprite = playerbar_green;
                }
                else if (ratio > 0.3f && playerbar_yellow != null)
                {
                    player_health_fill_image.sprite = playerbar_yellow;
                }
                else if (playerbar_red != null)
                {
                    player_health_fill_image.sprite = playerbar_red;
                }
            }
        }

        if (ammo_text != null)
        {
            string current_ammo;
            if (player_combat.active_weapon != null)
            {
                current_ammo = player_combat.active_weapon.GetAmmoDisplayText(player_ammo);
            }
            else if (player_combat.active_weapon_type == WeaponType.Melee)
            {
                current_ammo = player_combat.melee_weapon != null ? player_combat.melee_weapon.weapon_name : "MELEE";
            }
            else
            {
                int inMag = player_ammo != null ? player_ammo.ammo_in_mag : 0;
                int carried = player_ammo != null ? player_ammo.ammo_carried : 0;
                current_ammo = inMag + " / " + carried;
            }

            if (current_ammo != last_ammo_str)
            {
                last_ammo_str = current_ammo;
                ammo_text.text = current_ammo;
            }
        }

        if (candy_text != null && GameManager.instance != null && GameManager.instance.candy_total != last_candy)
        {
            last_candy = GameManager.instance.candy_total;
            candy_text.text = last_candy.ToString();
        }

        if (repair_points_text != null && player_combat.repair_points != last_repair)
        {
            last_repair = player_combat.repair_points;
            repair_points_text.text = last_repair.ToString();
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

        if (consumable_slot1_text != null && c1_count != last_c1_count)
        {
            last_c1_count = c1_count;
            consumable_slot1_text.text = c1_count.ToString();
        }

        if (consumable_slot2_text != null && c2_count != last_c2_count)
        {
            last_c2_count = c2_count;
            consumable_slot2_text.text = c2_count.ToString();
        }

        if (consumable_slot1_icon != null && c1_name != last_c1_name)
        {
            last_c1_name = c1_name;
            Sprite s1 = GetConsumableSprite(c1_name);
            consumable_slot1_icon.sprite = s1;
            consumable_slot1_icon.enabled = s1 != null;
        }

        if (consumable_slot2_icon != null && c2_name != last_c2_name)
        {
            last_c2_name = c2_name;
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
}
