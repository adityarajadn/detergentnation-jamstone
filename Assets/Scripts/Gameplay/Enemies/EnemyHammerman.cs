using System.Collections;
using UnityEngine;

public class EnemyHammerman : EnemyBase
{
    [SerializeField] private float hammer_aoe_multiplier = 1.6f;

    protected override IEnumerator AttackRoutine()
    {
        is_attacking = true;
        float attack_rate = enemy_data != null ? enemy_data.attack_rate : 1.6f;
        next_attack_time = Time.time + attack_rate;

        Color original_color = sr != null ? sr.color : Color.white;

        // Slow heavy windup
        float windup_time = 0.45f;
        float elapsed = 0f;
        while (elapsed < windup_time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windup_time;
            if (visual_transform != null)
            {
                visual_transform.localPosition = new Vector3(0f, t * 0.35f, 0f);
            }
            if (sr != null) sr.color = Color.Lerp(original_color, Color.red, t);
            yield return null;
        }

        // Slam down!
        if (visual_transform != null) visual_transform.localPosition = Vector3.zero;
        PerformMeleeStrike();

        yield return new WaitForSeconds(0.2f);
        if (sr != null) sr.color = original_color;
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
