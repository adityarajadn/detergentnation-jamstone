using UnityEngine;

public class ConsumableEnergyDrink : ConsumableBase
{
    [SerializeField] private float speed_multiplier = 1.4f;
    [SerializeField] private float duration = 8f;

    private void Reset()
    {
        consumable_name = "Energy Drink";
        popup_label = "SPEED UP!";
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "SPEED UP!";
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;
        PlayerController pc = combat.GetComponent<PlayerController>();
        if (pc == null) return false;

        pc.ApplySpeedBoost(speed_multiplier, duration);
        AudioManager.PlaySFX(GameConstants.AudioKeys.PlayerDrink);
        return true;
    }
}
