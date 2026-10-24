using System.Collections;
using UnityEngine;

public class EnemyWaffare : EnemyBase
{
    [SerializeField] private float frenzy_speed_multiplier = 1.35f;

    protected override void MoveTowardsTarget()
    {
        if (target_transform == null || rb == null || enemy_data == null) return;

        Vector2 dir = (target_transform.position - transform.position).normalized;
        float speed = enemy_data.move_speed;

        if (player_target != null)
        {
            speed *= frenzy_speed_multiplier;
        }

        rb.linearVelocity = new Vector2(Mathf.Sign(dir.x) * speed, rb.linearVelocity.y);
        UpdateFacing(dir.x);
    }

    protected override IEnumerator AttackRoutine()
    {
        is_attacking = true;
        float attack_rate = enemy_data != null ? enemy_data.attack_rate : 0.8f;
        next_attack_time = Time.time + attack_rate;

        Color original_color = sr != null ? sr.color : Color.white;
        Vector3 original_scale = transform.localScale;

        // Strike 1
        yield return new WaitForSeconds(0.12f);
        PerformMeleeStrike();

        // Combo pause
        yield return new WaitForSeconds(0.15f);

        // Strike 2
        PerformMeleeStrike();

        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = original_color;
        transform.localScale = original_scale;
        is_attacking = false;
    }
}
