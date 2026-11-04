using UnityEngine;

public class ConsumableSonarBatarang : ConsumableBase
{
    [SerializeField] private GameObject batarang_projectile_prefab;

    private void Reset()
    {
        consumable_name = GameConstants.EquipmentNames.ConsumableSonarBatarang;
        popup_label = "SONAR!";
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "SONAR!";
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;

        GameObject prefab = batarang_projectile_prefab;
        if (prefab == null) return false;

        Vector3 origin = combat.fire_point != null ? combat.fire_point.position : combat.transform.position;
        GameObject bat = Instantiate(prefab, origin, Quaternion.identity);
        Vector2 fly_dir = ((Vector2)(target_world_pos - origin)).normalized;

        SonarBatarangProjectile sbp = bat.GetComponent<SonarBatarangProjectile>();
        if (sbp != null) sbp.Launch(fly_dir);

        AudioManager.PlaySFX(GameConstants.AudioKeys.WhistleBlow);

        return true;
    }
}
