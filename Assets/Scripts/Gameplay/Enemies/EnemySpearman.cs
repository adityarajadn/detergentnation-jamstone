using UnityEngine;

public class EnemySpearman : EnemyBase
{
    [SerializeField] private float thrust_reach_multiplier = 1.4f;

    protected override void PerformMeleeStrike()
    {
        if (enemy_data == null) return;

        float facing_dir = transform.localScale.x >= 0 ? 1f : -1f;
        float range = enemy_data.attack_range * thrust_reach_multiplier;
        Vector2 attack_center = (Vector2)transform.position + new Vector2(facing_dir * (range * 0.65f), 0f);
        float attack_radius = Mathf.Max(0.7f, range * 0.5f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attack_center, attack_radius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit.CompareTag(GameConstants.Tags.Player) || hit.CompareTag(GameConstants.Tags.Minimarket) || hit.CompareTag(GameConstants.Tags.Defense))
            {
                Health target_h = hit.GetComponent<Health>();
                if (target_h != null && !target_h.is_dead)
                {
                    target_h.TakeDamage(enemy_data.attack_damage);
                }
            }
        }
    }
}
