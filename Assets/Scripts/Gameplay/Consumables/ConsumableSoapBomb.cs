using UnityEngine;

public class ConsumableSoapBomb : ConsumableBase
{
    [SerializeField] private GameObject soap_bomb_projectile_prefab;
    [SerializeField] private Vector2 throw_impulse = new Vector2(10f, 6f);

    private void Reset()
    {
        consumable_name = GameConstants.EquipmentNames.DefaultConsumable1;
        price = 100;
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;

        GameObject prefab = soap_bomb_projectile_prefab;
        if (prefab == null) return false;

        Vector3 origin = combat.fire_point != null ? combat.fire_point.position : combat.transform.position;
        GameObject bomb = Instantiate(prefab, origin, Quaternion.identity);
        Vector2 throw_dir = ((Vector2)(target_world_pos - origin)).normalized * throw_impulse.x + Vector2.up * throw_impulse.y;

        SoapBombProjectile sbp = bomb.GetComponent<SoapBombProjectile>();
        if (sbp != null) sbp.Launch(throw_dir);
        else
        {
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = throw_dir;
        }

        return true;
    }
}
