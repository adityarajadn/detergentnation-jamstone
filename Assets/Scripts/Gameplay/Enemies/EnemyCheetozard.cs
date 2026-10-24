using UnityEngine;

public class EnemyCheetozard : EnemyBase
{
    [SerializeField] private float projectile_speed = 9f;

    protected override void PerformRangedStrike()
    {
        if (enemy_data == null || enemy_data.projectile_prefab == null) return;

        Vector2 dir = target_transform != null
            ? (target_transform.position - transform.position).normalized
            : (transform.localScale.x >= 0 ? Vector2.right : Vector2.left);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject proj_obj = Instantiate(enemy_data.projectile_prefab, transform.position, Quaternion.Euler(0f, 0f, angle));
        Projectile proj = proj_obj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.damage = enemy_data.attack_damage;
            proj.speed = projectile_speed;
            proj.is_enemy_projectile = true;
        }
    }
}
