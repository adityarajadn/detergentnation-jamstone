using UnityEngine;

public class ConsumableEnergyDrink : ConsumableBase
{
    [SerializeField] private float speed_multiplier = 1.4f;
    [SerializeField] private float duration = 8f;

    private void Reset()
    {
        consumable_name = "Energy Drink";
        price = 10;
        popup_label = "SPEED UP!";
        popup_color = new Color(0.22f, 0.74f, 0.97f, 1f);
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "SPEED UP!";
        if (popup_color == Color.white) popup_color = new Color(0.22f, 0.74f, 0.97f, 1f);
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;
        PlayerController pc = combat.GetComponent<PlayerController>();
        if (pc == null) return false;

        pc.ApplySpeedBoost(speed_multiplier, duration);
        AudioManager.PlaySFX("player_drink");
        return true;
    }
}
