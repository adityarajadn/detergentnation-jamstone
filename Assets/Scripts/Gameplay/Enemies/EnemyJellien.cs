using System.Collections;
using UnityEngine;

public class EnemyJellien : EnemyBase
{
    [SerializeField] private float leap_force = 3f;

    protected override IEnumerator AttackRoutine()
    {
        is_attacking = true;
        float attack_rate = enemy_data != null ? enemy_data.attack_rate : 1f;
        next_attack_time = Time.time + attack_rate;

        Color original_color = sr != null ? sr.color : Color.white;
        Vector3 original_scale = transform.localScale;

        // Jelly squash anticipation
        float windup_time = 0.22f;
        float elapsed = 0f;
        while (elapsed < windup_time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windup_time;
            if (visual_transform != null)
            {
                visual_transform.localScale = new Vector3(1f + t * 0.2f, 1f - t * 0.2f, 1f);
            }
            if (sr != null) sr.color = Color.Lerp(original_color, Color.red, t * 0.7f);
            yield return null;
        }

        // Jelly hop forward
        float facing_dir = transform.localScale.x >= 0 ? 1f : -1f;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(facing_dir * leap_force, 2.5f);
        }

        PerformMeleeStrike();

        // Recovery
        yield return new WaitForSeconds(0.15f);
        if (visual_transform != null) visual_transform.localScale = Vector3.one;
        if (sr != null) sr.color = original_color;
        transform.localScale = original_scale;
        is_attacking = false;
    }
}
