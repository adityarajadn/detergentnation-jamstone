using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemy_data;

    private Health health;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform target_transform;
    private Health target_health;
    private Transform player_target;
    private float next_attack_time = 0f;
    private bool is_attacking = false;
    private bool is_stunned = false;
    private float stun_timer = 0f;

    private void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        int en_layer = LayerMask.NameToLayer("Enemy");
        int pl_layer = LayerMask.NameToLayer("Player");
        if (en_layer != -1)
        {
            Physics2D.IgnoreLayerCollision(en_layer, en_layer, true);
            if (pl_layer != -1) Physics2D.IgnoreLayerCollision(en_layer, pl_layer, true);
        }
    }

    private void Start()
    {
        if (enemy_data != null)
        {
            health.SetMaxHealth(enemy_data.health_max, true);
        }

        CircleCollider2D aggro_col = GetComponentInChildren<EnemyAggroDetector>()?.GetComponent<CircleCollider2D>();
        if (aggro_col != null && enemy_data != null)
        {
            aggro_col.radius = enemy_data.aggro_range;
        }

        health.on_death.AddListener(OnDeath);
        FindTarget();
    }

    private void Update()
    {
        if (health.is_dead) return;

        if (is_stunned)
        {
            rb.linearVelocity = Vector2.zero;
            stun_timer -= Time.deltaTime;
            if (stun_timer <= 0f)
            {
                is_stunned = false;
                if (sr != null) sr.color = enemy_data != null ? enemy_data.body_color : Color.white;
            }
            return;
        }

        if (is_attacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckPathObstacles();

        if (player_target != null)
        {
            Health p_health = player_target.GetComponent<Health>();
            if (p_health != null && p_health.is_dead)
            {
                player_target = null;
            }
        }

        FindTarget();
        if (target_transform == null) return;

        float dist = Vector2.Distance(transform.position, target_transform.position);

        if (dist <= enemy_data.attack_range)
        {
            rb.linearVelocity = Vector2.zero;
            if (Time.time >= next_attack_time)
            {
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    private void CheckPathObstacles()
    {
        float facing_dir = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 check_pos = (Vector2)transform.position + new Vector2(facing_dir * 0.8f, 0f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(check_pos, 0.7f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag("Defense"))
            {
                Health def_health = hits[i].GetComponent<Health>();
                if (def_health != null && !def_health.is_dead)
                {
                    target_transform = hits[i].transform;
                    target_health = def_health;
                    return;
                }
            }
        }
    }

    private void FindTarget()
    {
        if (player_target != null)
        {
            target_transform = player_target;
            target_health = player_target.GetComponent<Health>();
            return;
        }

        if (Minimarket.instance != null && Minimarket.instance.health != null && !Minimarket.instance.health.is_dead)
        {
            target_transform = Minimarket.instance.transform;
            target_health = Minimarket.instance.health;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target_transform = player.transform;
            target_health = player.GetComponent<Health>();
        }
    }

    public void OnPlayerEnteredAggro(Transform player)
    {
        player_target = player;
        FindTarget();
    }

    public void OnPlayerExitedAggro(Transform player)
    {
        if (player_target == player)
        {
            player_target = null;
            FindTarget();
        }
    }

    public void ApplyStun(float duration)
    {
        is_stunned = true;
        stun_timer = Mathf.Max(stun_timer, duration);
        rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = new Color(0.4f, 0.8f, 1f, 1f);
    }

    private void MoveTowardsTarget()
    {
        Vector2 dir = (target_transform.position - transform.position).normalized;

        if (enemy_data.is_flying)
        {
            rb.linearVelocity = dir * enemy_data.move_speed;
        }
        else
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(dir.x) * enemy_data.move_speed, rb.linearVelocity.y);
        }

        if (dir.x > 0.05f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (dir.x < -0.05f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private IEnumerator AttackRoutine()
    {
        is_attacking = true;
        next_attack_time = Time.time + enemy_data.attack_rate;

        Color original_color = sr != null ? sr.color : Color.white;
        Vector3 original_scale = transform.localScale;

        // 1. Wind-up anticipation
        float windup_time = 0.22f;
        float elapsed = 0f;
        while (elapsed < windup_time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windup_time;
            if (sr != null) sr.color = Color.Lerp(original_color, Color.red, t * 0.7f);
            yield return null;
        }

        // 2. Active Attack Strike Hitbox
        if (enemy_data.attack_type == EnemyAttackType.Ranged && enemy_data.projectile_prefab != null)
        {
            Vector2 dir = target_transform != null ? (target_transform.position - transform.position).normalized : (transform.localScale.x >= 0 ? Vector2.right : Vector2.left);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            GameObject proj_obj = Instantiate(enemy_data.projectile_prefab, transform.position, Quaternion.Euler(0f, 0f, angle));
            Projectile proj = proj_obj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.damage = enemy_data.attack_damage;
                proj.is_enemy_projectile = true;
            }
        }
        else
        {
            PerformMeleeStrike();
        }

        // 3. Recovery
        yield return new WaitForSeconds(0.12f);
        if (sr != null) sr.color = original_color;
        transform.localScale = original_scale;
        is_attacking = false;
    }

    private void PerformMeleeStrike()
    {
        float facing_dir = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 attack_center = (Vector2)transform.position + new Vector2(facing_dir * (enemy_data.attack_range * 0.6f), 0f);
        float attack_radius = Mathf.Max(0.7f, enemy_data.attack_range * 0.7f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attack_center, attack_radius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit.CompareTag("Player") || hit.CompareTag("Minimarket") || hit.CompareTag("Defense"))
            {
                Health target_h = hit.GetComponent<Health>();
                if (target_h != null && !target_h.is_dead)
                {
                    target_h.TakeDamage(enemy_data.attack_damage);
                }
            }
        }
    }

    private void OnDeath()
    {
        if (KillStreakManager.instance != null)
        {
            KillStreakManager.instance.RegisterKill();
        }

        if (DropSystem.instance != null && enemy_data != null)
        {
            DropSystem.instance.SpawnDrops(transform.position, enemy_data);
        }

        if (WaveManager.instance != null)
        {
            WaveManager.instance.OnEnemyKilled(this);
        }

        Destroy(gameObject);
    }
}
