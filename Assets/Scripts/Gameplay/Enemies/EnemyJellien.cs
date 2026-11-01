using System.Collections;
using UnityEngine;

public class EnemyJellien : EnemyBase
{
    [SerializeField] private float leap_force = 3f;

    protected override IEnumerator AttackRoutine()
    {
        is_attacking = true;
        if (animator != null)
        {
            animator.SetBool("is_moving", false);
            animator.SetTrigger("attack");
        }

        float attack_rate = enemy_data != null ? enemy_data.attack_rate : 1f;
        next_attack_time = Time.time + attack_rate;

        // 1. Wind-up anticipation (no scale tampering, no red tint)
        yield return new WaitForSeconds(0.22f);

        // 2. Jelly hop forward
        float facing_dir = visual_transform != null && visual_transform.localScale.x < 0 ? 1f : -1f;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(facing_dir * leap_force, 2.5f);
        }

        AudioManager.PlaySFX("enemy_attack");
        PerformMeleeStrike();

        // 3. Recovery
        yield return new WaitForSeconds(0.15f);
        is_attacking = false;
    }
}
