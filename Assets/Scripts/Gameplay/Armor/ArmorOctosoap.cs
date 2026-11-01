using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArmorOctosoap : ArmorBase
{
    [SerializeField] private GameObject soap_projectile_prefab;
    [SerializeField] private float throw_interval = 15f;
    [SerializeField] private int soap_damage = 30;
    [SerializeField] private float max_throw_distance = 8f;
    [SerializeField] private float flight_duration = 0.55f;
    [SerializeField] private float arc_height = 2.0f;
    [SerializeField] private LayerMask obstacle_mask;

    private Coroutine throw_coroutine;
    private Camera main_camera;

    private void Reset()
    {
        armor_name = GameConstants.EquipmentNames.ArmorOctosoap;
        armor_rating = 3;
        obstacle_mask = LayerMask.GetMask(GameConstants.Layers.Ground);
    }

    protected override void Awake()
    {
        base.Awake();
        armor_name = GameConstants.EquipmentNames.ArmorOctosoap;
        armor_rating = 3;
        if (obstacle_mask.value == 0)
        {
            obstacle_mask = LayerMask.GetMask(GameConstants.Layers.Ground);
        }
    }

    private void OnEnable()
    {
        throw_coroutine = StartCoroutine(AutoThrowRoutine());
    }

    private void OnDisable()
    {
        if (throw_coroutine != null)
        {
            StopCoroutine(throw_coroutine);
            throw_coroutine = null;
        }
    }

    private IEnumerator AutoThrowRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(throw_interval);
            ThrowSoap();
        }
    }

    private void ThrowSoap()
    {
        if (soap_projectile_prefab == null) return;

        PlayerCombat combat = GetComponentInParent<PlayerCombat>();
        if (combat == null) combat = FindFirstObjectByType<PlayerCombat>();

        Vector3 origin = combat != null && combat.fire_point != null
            ? combat.fire_point.position
            : transform.position;

        if (main_camera == null) main_camera = Camera.main;

        Vector3 target_world_pos = origin + Vector3.right * 4f;
        if (main_camera != null && Mouse.current != null)
        {
            target_world_pos = main_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            target_world_pos.z = 0f;
        }
        else
        {
            Transform player = transform.root;
            Transform visuals = player != null ? player.Find("Visuals") : null;
            if ((visuals != null && visuals.localScale.x < 0) || (player != null && player.localScale.x < 0))
            {
                target_world_pos = origin + Vector3.left * 4f;
            }
        }

        Vector2 diff = target_world_pos - origin;
        float dist = diff.magnitude;
        Vector2 dir = dist > 0.001f ? diff / dist : Vector2.right;

        float target_dist = Mathf.Min(dist, max_throw_distance);
        Vector3 final_target = (Vector2)origin + dir * target_dist;

        int mask = obstacle_mask.value == 0 ? LayerMask.GetMask(GameConstants.Layers.Ground) : obstacle_mask.value;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, target_dist, mask);
        if (hit.collider != null)
        {
            final_target = hit.point - dir * 0.2f;
        }

        GameObject soap = Instantiate(soap_projectile_prefab, origin, Quaternion.identity);
        SoapBombProjectile sbp = soap.GetComponent<SoapBombProjectile>();
        if (sbp != null)
        {
            sbp.damage = soap_damage;
            sbp.LaunchArc(final_target, flight_duration, arc_height);
        }
        else
        {
            Rigidbody2D rb = soap.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * 10f + Vector2.up * 6f;
        }
    }
}
