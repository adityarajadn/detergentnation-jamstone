using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    public string enemyType = "jellien";
    public string attackType = "Melee"; // Melee, Ranged
    public string category = "Normal";  // Normal, Tiny, Brawler, Brute, Tank
    public int healthMax = 60;
    public int healthCurrent = 60;
    public float moveSpeed = 1.8f;
    public int attackDamage = 10;
    public float attackDelay = 1.0f;
    public float attackRange = 0.8f;
    public float rangedRange = 6.0f;
    public bool isFlying = false;
    public int candyReward = 15;

    public Slider hpSlider;
    public SpriteRenderer spriteRenderer;
    public GameObject dropItemPrefab;
    public GameObject enemyProjectilePrefab;

    private float attackCooldown = 0f;
    private float stunTimer = 0f;
    private float hitstopTimer = 0f;
    private Rigidbody2D rb;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (isFlying && rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        healthCurrent = healthMax;
        if (hpSlider != null)
        {
            hpSlider.maxValue = healthMax;
            hpSlider.value = healthCurrent;
        }
        var market = FindAnyObjectByType<Minimarket>();
        if (market != null) target = market.transform;
    }

    private void Update()
    {
        if (hitstopTimer > 0f) { hitstopTimer -= Time.deltaTime; return; }
        if (stunTimer > 0f) { stunTimer -= Time.deltaTime; return; }
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;

        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            bool isRanged = attackType.ToLower() == "ranged" || enemyType == "cheetozard" || enemyType == "marshquito";
            if (isRanged && dist <= rangedRange)
            {
                if (attackCooldown <= 0f) ExecuteRangedAttack();
                return;
            }
            else if (!isRanged && dist <= attackRange)
            {
                if (attackCooldown <= 0f) ExecuteAttack();
                return;
            }
        }

        // Bergerak menuju Minimarket di posisi X = 0
        float dir = transform.position.x > 0 ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, isFlying ? 0 : rb.linearVelocity.y);

        Vector3 s = transform.localScale;
        s.x = dir > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    public void ExecuteAttack()
    {
        attackCooldown = attackDelay;
        AudioManager.Instance?.PlaySfx("hit");
        if (target != null && target.TryGetComponent<Minimarket>(out var store))
        {
            store.TakeDamage(attackDamage);
        }
    }

    public void ExecuteRangedAttack()
    {
        attackCooldown = attackDelay * 1.5f;
        if (enemyProjectilePrefab != null)
        {
            GameObject p = Instantiate(enemyProjectilePrefab, transform.position + new Vector3(0, -0.2f, 0), Quaternion.identity);
            Vector2 dir = target != null ? (target.position - transform.position).normalized : Vector2.left;
            p.GetComponent<EnemyProjectile>()?.Setup(dir, attackDamage);
            AudioManager.Instance?.PlaySfx("shoot");
        }
    }

    public void Stun(float duration) => stunTimer = duration;

    public void TakeDamage(int dmg, Vector2 hitSource = default)
    {
        healthCurrent = Mathf.Max(0, healthCurrent - dmg);
        if (hpSlider != null) hpSlider.value = healthCurrent;

        hitstopTimer = 0.06f;
        Vector2 kbDir = hitSource != default ? ((Vector2)transform.position - hitSource).normalized : new Vector2(transform.position.x > 0 ? 1 : -1, 0.4f);
        float kbFactor = category.ToLower() == "tank" ? 0.25f : (1.0f / Mathf.Max(0.5f, rb != null ? rb.mass : 1f));
        rb.linearVelocity = kbDir * (3.0f * kbFactor);

        AudioManager.Instance?.PlaySfx("hit");

        if (healthCurrent <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EventBus.RaiseEnemyKilled(enemyType, transform.position);
        SpawnDrops();
        Destroy(gameObject);
    }

    private void SpawnDrops()
    {
        if (Random.value <= 0.7f && dropItemPrefab != null)
        {
            GameObject d = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            d.GetComponent<DropItem>()?.Setup("candy", candyReward);
        }
    }
}
