using UnityEngine;

public class ConsumableToolsBox : ConsumableBase
{
    [SerializeField] private int repair_points = 30;
    [SerializeField] private int minimarket_heal = 30;

    private void Reset()
    {
        consumable_name = "Tools Box";
        price = 25;
        popup_label = "+REPAIR";
        popup_color = new Color(0.98f, 0.57f, 0.24f, 1f);
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "+REPAIR";
        if (popup_color == Color.white) popup_color = new Color(0.98f, 0.57f, 0.24f, 1f);
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;

        combat.AddRepairPoints(repair_points);
        if (Minimarket.instance != null && Minimarket.instance.health != null)
        {
            Minimarket.instance.health.Heal(minimarket_heal);
        }

        AudioManager.PlaySFX(GameConstants.AudioKeys.Repair);
        return true;
    }
}
