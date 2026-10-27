using UnityEngine;

public class WeaponBubbleStick : MeleeWeapon
{
    [SerializeField] private GameObject bubble_projectile_prefab;

    public override void PerformStrike(Vector3 origin, float range, int damage, LayerMask enemy_layer)
    {
        base.PerformStrike(origin, range, damage, enemy_layer);

        GameObject prefab = bubble_projectile_prefab != null ? bubble_projectile_prefab : (weapon_data != null ? weapon_data.projectile_prefab : null);
        if (prefab != null)
        {
            Vector3 spawn_pos = melee_point != null ? melee_point.position : origin;
            GameObject bubble_obj = Instantiate(prefab, spawn_pos, Quaternion.identity);
            Projectile proj = bubble_obj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.damage = Mathf.RoundToInt(damage * 0.6f);
                proj.is_floating_bubble = true;
                proj.lifetime = 10f;
                proj.is_enemy_projectile = false;
            }
        }
    }
}
