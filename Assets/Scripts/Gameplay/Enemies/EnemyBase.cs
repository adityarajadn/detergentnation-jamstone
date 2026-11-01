using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public EnemyData enemy_data;

    [Header("Visual & Effects")]
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected Transform visual_transform;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Color freeze_tint_color = new Color(0.4f, 0.8f, 1f, 1f);

    protected Health health;
    protected Rigidbody2D rb;

    protected Transform target_transform;
    protected Health target_health;
    protected Transform player_target;
    protected float next_attack_time = 0f;
    protected bool is_attacking = false;
    protected bool is_stunned = false;
    protected float stun_timer = 0f;

    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();

        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (visual_transform == null && sr != null)
        {
            visual_transform = sr.transform;
        }

        int en_layer = LayerMask.NameToLayer(GameConstants.Layers.Enemy);
        if (en_layer != -1)
        {
            Physics2D.IgnoreLayerCollision(en_layer, en_layer, true);
        }
    }

    protected virtual void Start()
    {
        if (enemy_data != null && health != null)
        {
            health.SetMaxHealth(enemy_data.health_max, true);
        }

        CircleCollider2D aggro_col = GetComponentInChildren<EnemyAggroDetector>()?.GetComponent<CircleCollider2D>();
        if (aggro_col != null && enemy_data != null)
        {
            aggro_col.radius = enemy_data.aggro_range;
        }

        if (health != null)
        {
            health.on_death.AddListener(OnDeath);
            health.on_health_changed.AddListener(OnHealthChanged);
        }

        FindTarget();
    }

    protected virtual void Update()
    {
        if (health != null && health.is_dead) return;

        if (is_stunned)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
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
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateTarget();
        if (target_transform == null) return;

        float dist = Vector2.Distance(transform.position, target_transform.position);

        float range = enemy_data != null ? enemy_data.attack_range : 1.2f;
        if (dist <= range)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("is_moving", false);
            UpdateFacing(target_transform.position.x - transform.position.x);
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

    protected virtual void UpdateTarget()
    {
        CheckPathObstacles();
        if (target_transform != null && target_transform.CompareTag(GameConstants.Tags.Defense))
        {
            if (target_health != null && !target_health.is_dead)
            {
                return;
            }
            target_transform = null;
            target_health = null;
        }

        if (player_target != null)
        {
            Health p_health = player_target.GetComponent<Health>();
            if (p_health != null && p_health.is_dead)
            {
                player_target = null;
            }
        }

        FindTarget();
    }

    protected virtual void CheckPathObstacles()
    {
        float facing_dir = visual_transform != null && visual_transform.localScale.x < 0 ? 1f : -1f;
        Vector2 check_pos = (Vector2)transform.position + new Vector2(facing_dir * 0.8f, 0f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(check_pos, 0.7f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag(GameConstants.Tags.Defense))
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

    protected virtual void FindTarget()
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

        GameObject player = PlayerController.Instance != null ? PlayerController.Instance.gameObject : GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
        if (player != null)
        {
            target_transform = player.transform;
            target_health = player.GetComponent<Health>();
        }
    }

    public virtual void OnPlayerEnteredAggro(Transform player)
    {
        player_target = player;
        FindTarget();
    }

    public virtual void OnPlayerExitedAggro(Transform player)
    {
        if (player_target == player)
        {
            player_target = null;
            FindTarget();
        }
    }

    public virtual void ApplyStun(float duration)
    {
        is_stunned = true;
        stun_timer = Mathf.Max(stun_timer, duration);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = freeze_tint_color;
    }

    protected virtual void MoveTowardsTarget()
    {
        if (target_transform == null || rb == null || enemy_data == null) return;

        Vector2 dir = (target_transform.position - transform.position).normalized;

        if (enemy_data.is_flying)
        {
            rb.linearVelocity = dir * enemy_data.move_speed;
        }
        else
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(dir.x) * enemy_data.move_speed, rb.linearVelocity.y);
        }

        if (animator != null) animator.SetBool("is_moving", true);
        UpdateFacing(dir.x);
    }

    protected virtual void UpdateFacing(float xDir)
    {
        if (visual_transform == null) return;
        float absX = Mathf.Abs(visual_transform.localScale.x);
        // Sprite default faces Left: right (xDir > 0) -> -absX, left (xDir < 0) -> +absX
        if (xDir > 0.05f)
        {
            visual_transform.localScale = new Vector3(-absX, visual_transform.localScale.y, visual_transform.localScale.z);
        }
        else if (xDir < -0.05f)
        {
            visual_transform.localScale = new Vector3(absX, visual_transform.localScale.y, visual_transform.localScale.z);
        }
    }

    protected virtual IEnumerator AttackRoutine()
    {
        is_attacking = true;
        if (animator != null)
        {
            animator.SetBool("is_moving", false);
            animator.SetTrigger("attack");
        }
        float attack_rate = enemy_data != null ? enemy_data.attack_rate : 1f;
        next_attack_time = Time.time + attack_rate;

        // 1. Wind-up anticipation (no red color tint)
        yield return new WaitForSeconds(0.22f);

        // 2. Active Strike Hitbox
        AudioManager.PlaySFX("enemy_attack");
        if (enemy_data != null && enemy_data.attack_type == EnemyAttackType.Ranged && enemy_data.projectile_prefab != null)
        {
            PerformRangedStrike();
        }
        else
        {
            PerformMeleeStrike();
        }

        // 3. Recovery
        yield return new WaitForSeconds(0.12f);
        is_attacking = false;
    }

    protected virtual void PerformMeleeStrike()
    {
        if (enemy_data == null) return;

        float facing_dir = visual_transform != null && visual_transform.localScale.x < 0 ? 1f : -1f;
        Vector2 attack_center = (Vector2)transform.position + new Vector2(facing_dir * (enemy_data.attack_range * 0.6f), 0f);
        float attack_radius = Mathf.Max(0.7f, enemy_data.attack_range * 0.7f);

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

    protected virtual void PerformRangedStrike()
    {
        if (enemy_data == null || enemy_data.projectile_prefab == null) return;

        Vector2 dir = target_transform != null
            ? (target_transform.position - transform.position).normalized
            : (transform.localScale.x >= 0 ? Vector2.right : Vector2.left);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject proj_obj = Instantiate(enemy_data.projectile_prefab, transform.position, Quaternion.Euler(0f, 0f, angle));
        Projectile proj = proj_obj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.damage = enemy_data.attack_damage;
            proj.is_enemy_projectile = true;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < max && current > 0)
        {
            AudioManager.PlaySFX(GameConstants.AudioKeys.EnemyHurt);
            if (animator != null && HasParameter(animator, "hit"))
            {
                animator.SetTrigger("hit");
            }
        }
    }

    protected virtual void OnDeath()
    {
        AudioManager.PlaySFX(GameConstants.AudioKeys.EnemyDeath);
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

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }

        if (animator != null && HasParameter(animator, "death"))
        {
            animator.SetTrigger("death");
            Destroy(gameObject, 0.75f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected bool HasParameter(Animator anim, string paramName)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return false;
        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == paramName) return true;
        }
        return false;
    }
}
