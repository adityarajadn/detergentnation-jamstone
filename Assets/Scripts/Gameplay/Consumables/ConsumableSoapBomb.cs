using UnityEngine;

public class ConsumableSoapBomb : ConsumableBase
{
    [SerializeField] private GameObject soap_bomb_projectile_prefab;
    [SerializeField] private float max_throw_distance = 8f;
    [SerializeField] private float arc_height = 2.5f;
    [SerializeField] private float flight_duration = 0.55f;
    [SerializeField] private LayerMask obstacle_mask = 0;

    private void Reset()
    {
        consumable_name = GameConstants.EquipmentNames.DefaultConsumable1;
        price = 100;
        popup_label = "SOAP BOMB!";
        popup_color = new Color(1f, 0.88f, 0.32f, 1f);
        obstacle_mask = LayerMask.GetMask(GameConstants.Layers.Ground);
    }

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(popup_label) || popup_label == "USED!") popup_label = "SOAP BOMB!";
        if (popup_color == Color.white) popup_color = new Color(1f, 0.88f, 0.32f, 1f);
    }

    public override bool Use(PlayerCombat combat, Vector3 target_world_pos)
    {
        if (combat == null) return false;

        GameObject prefab = soap_bomb_projectile_prefab;
        if (prefab == null) return false;

        Vector3 origin = combat.fire_point != null ? combat.fire_point.position : combat.transform.position;
        Vector2 diff = target_world_pos - origin;
        float dist = diff.magnitude;
        Vector2 dir = dist > 0.001f ? diff / dist : Vector2.right;

        float target_dist = Mathf.Min(dist, max_throw_distance);
        Vector3 final_target = (Vector2)origin + dir * target_dist;

        // Raycast check for obstacles (e.g. Ground / Walls)
        int mask = obstacle_mask.value == 0 ? LayerMask.GetMask(GameConstants.Layers.Ground) : obstacle_mask.value;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, target_dist, mask);
        if (hit.collider != null)
        {
            final_target = hit.point - dir * 0.2f;
        }

        GameObject bomb = Instantiate(prefab, origin, Quaternion.identity);
        SoapBombProjectile sbp = bomb.GetComponent<SoapBombProjectile>();
        if (sbp != null)
        {
            sbp.LaunchArc(final_target, flight_duration, arc_height);
        }
        else
        {
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * 10f + Vector2.up * 6f;
        }

        return true;
    }
}
