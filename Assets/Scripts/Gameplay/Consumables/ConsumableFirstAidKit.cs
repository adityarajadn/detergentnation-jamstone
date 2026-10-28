using UnityEngine;

public class ConsumableFirstAidKit : ConsumableBase
{
    [SerializeField] private int heal_amount = 50;

    private void Reset()
    {
        consumable_name = GameConstants.EquipmentNames.DefaultConsumable2;
        price = 25;
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;
        Health hp = combat.GetComponent<Health>();
        if (hp == null) return false;

        hp.Heal(heal_amount);
        AudioManager.PlaySFX("consumable_healing");
        return true;
    }
}
