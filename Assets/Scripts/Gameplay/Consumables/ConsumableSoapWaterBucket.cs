using UnityEngine;

public class ConsumableSoapWaterBucket : ConsumableBase
{
    [SerializeField] private int shield_charges = 3;

    private void Reset()
    {
        consumable_name = "Soap Water Bucket";
        price = 75;
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;
        Health hp = combat.GetComponent<Health>();
        if (hp == null) return false;

        hp.shield_charges = shield_charges;
        return true;
    }
}
