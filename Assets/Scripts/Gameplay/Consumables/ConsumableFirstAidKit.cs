using UnityEngine;

public class ConsumableFirstAidKit : ConsumableBase
{
    [SerializeField] private int heal_amount = 50;

    private void Reset()
    {
        consumable_name = GameConstants.EquipmentNames.ConsumableFirstAidKit;
        popup_label = "+50 HP";
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "+50 HP";
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;
        Health hp = combat.GetComponent<Health>();
        if (hp == null) return false;

        hp.Heal(heal_amount);
        AudioManager.PlaySFX(GameConstants.AudioKeys.PlayerHeal);
        return true;
    }
}
