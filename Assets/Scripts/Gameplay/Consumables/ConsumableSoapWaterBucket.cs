using UnityEngine;

public class ConsumableSoapWaterBucket : ConsumableBase
{
    [SerializeField] private int shield_charges = 3;

    private void Reset()
    {
        consumable_name = "Soap Water Bucket";
        popup_label = "SOAP BUCKET!";
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "SOAP BUCKET!";
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
