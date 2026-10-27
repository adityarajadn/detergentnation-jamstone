using UnityEngine;

public class ArmorWorkersHat : ArmorBase
{
    private void Reset()
    {
        armor_name = GameConstants.EquipmentNames.ArmorWorkersHat;
        armor_rating = 1;
    }

    protected override void Awake()
    {
        base.Awake();
        armor_name = GameConstants.EquipmentNames.ArmorWorkersHat;
        armor_rating = 1;
    }
}
