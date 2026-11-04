using System.Collections;
using UnityEngine;

public class EnemyHammerman : EnemyBase
{
    [SerializeField] private float hammer_aoe_multiplier = 1.6f;

    protected override IEnumerator AttackRoutine()
    {
        is_attacking = true;
        if (animator != null)
        {
            animator.SetBool("is_moving", false);
            animator.SetTrigger("attack");
        }

        float attack_rate = enemy_data != null ? enemy_data.attack_rate : 1.6f;
        next_attack_time = Time.time + attack_rate;

        // Slow heavy windup (no red tint)
        yield return new WaitForSeconds(0.45f);

        // Slam down!
        AudioManager.PlaySFX(GameConstants.AudioKeys.EnemyAttack);
        PerformMeleeStrike();

        yield return new WaitForSeconds(0.2f);
        is_attacking = false;
    }

    protected override void PerformMeleeStrike()
    {
        if (enemy_data == null) return;

        float facing_dir = transform.localScale.x >= 0 ? 1f : -1f;
        float range = enemy_data.attack_range * hammer_aoe_multiplier;
        Vector2 attack_center = (Vector2)transform.position + new Vector2(facing_dir * (range * 0.5f), -0.2f);
        float attack_radius = Mathf.Max(1.2f, range * 0.7f);

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
