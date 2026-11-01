using UnityEngine;

public class ConsumableSoapWaterBucket : ConsumableBase
{
    [SerializeField] private int shield_charges = 3;

    private void Reset()
    {
        consumable_name = "Soap Water Bucket";
        price = 75;
        popup_label = "SOAP BUCKET!";
        popup_color = new Color(0.4f, 0.9f, 1f, 1f);
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "SOAP BUCKET!";
        if (popup_color == Color.white) popup_color = new Color(0.4f, 0.9f, 1f, 1f);
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
