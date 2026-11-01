using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("Ranged Setup")]
    public Transform fire_point;

    protected override void Awake()
    {
        base.Awake();
        if (fire_point == null)
        {
            Transform fp = transform.Find("FirePoint");
            fire_point = fp != null ? fp : transform;
        }
    }

    public virtual void Fire(Vector2 aim_dir, int damage_bonus)
    {
        if (weapon_data == null || weapon_data.projectile_prefab == null) return;

        int total_dmg = weapon_data.damage + damage_bonus;
        Vector3 spawn_pos = fire_point != null ? fire_point.position : transform.position;

        if (weapon_data.pellet_count > 1)
        {
            float start_angle = -weapon_data.spread_angle * 0.5f;
            float step = weapon_data.spread_angle / (weapon_data.pellet_count - 1);
            float base_angle = Mathf.Atan2(aim_dir.y, aim_dir.x) * Mathf.Rad2Deg;

            for (int i = 0; i < weapon_data.pellet_count; i++)
            {
                float ang = (base_angle + start_angle + i * step) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                SpawnProjectile(spawn_pos, dir, total_dmg);
            }
        }
        else
        {
            SpawnProjectile(spawn_pos, aim_dir, total_dmg);
        }
    }

    public override string GetAmmoDisplayText(PlayerAmmo ammo)
    {
        int inMag = ammo != null ? ammo.ammo_in_mag : 0;
        int carried = ammo != null ? ammo.ammo_carried : 0;
        return inMag + " / " + carried;
    }

    protected virtual void SpawnProjectile(Vector3 pos, Vector2 dir, int dmg)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject proj_obj = Instantiate(weapon_data.projectile_prefab, pos, Quaternion.Euler(0f, 0f, angle));
        Projectile proj = proj_obj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.damage = dmg;
            proj.speed = weapon_data.projectile_speed;
            proj.lifetime = weapon_data.projectile_lifetime;
            proj.is_floating_bubble = weapon_data.is_floating_bubble;
            proj.is_enemy_projectile = false;
        }
    }
}
