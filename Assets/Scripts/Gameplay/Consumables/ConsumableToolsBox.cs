using UnityEngine;

public class ConsumableToolsBox : ConsumableBase
{
    [SerializeField] private int repair_points = 30;
    [SerializeField] private int minimarket_heal = 30;

    private void Reset()
    {
        consumable_name = "Tools Box";
        price = 25;
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;

        combat.AddRepairPoints(repair_points);
        if (Minimarket.instance != null && Minimarket.instance.health != null)
        {
            Minimarket.instance.health.Heal(minimarket_heal);
        }

        AudioEvents.TriggerConsumableRepair();
        return true;
    }
}
