using UnityEngine;

public class EnemyMarshquito : EnemyBase
{
    [SerializeField] private float hover_wobble_freq = 6f;
    [SerializeField] private float hover_wobble_amp = 0.8f;
    [SerializeField] private float preferred_standoff_distance = 4.5f;

    protected override void MoveTowardsTarget()
    {
        if (target_transform == null || rb == null || enemy_data == null) return;

        Vector2 to_target = (Vector2)target_transform.position - (Vector2)transform.position;
        float dist = to_target.magnitude;
        Vector2 dir = to_target.normalized;

        float wobble_y = Mathf.Sin(Time.time * hover_wobble_freq) * hover_wobble_amp;
        Vector2 move_velocity;

        if (dist < preferred_standoff_distance * 0.8f)
        {
            move_velocity = -dir * (enemy_data.move_speed * 0.75f) + new Vector2(0f, wobble_y);
        }
        else
        {
            move_velocity = dir * enemy_data.move_speed + new Vector2(0f, wobble_y);
        }

        rb.linearVelocity = move_velocity;
        UpdateFacing(dir.x);
    }
}
