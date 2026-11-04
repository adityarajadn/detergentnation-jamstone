using DG.Tweening;
using UnityEngine;

public class SonarBatarangProjectile : MonoBehaviour
{
    private const float SpinTurnSeconds = 0.5f;

    public float speed = 12f;
    public float stun_radius = 4.0f;
    public float stun_duration = 4.0f;
    public float lifetime = 3.0f;
    public GameObject hit_effect_prefab;

    private Rigidbody2D rb;
    private float timer = 0f;
    private bool has_detonated = false;
    private Tween spin_tween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    private void Start()
    {
        // 720 degrees per second = one full turn every SpinTurnSeconds
        spin_tween = transform
            .DORotate(new Vector3(0f, 0f, 360f), SpinTurnSeconds, RotateMode.FastBeyond360)
            .SetRelative()
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    private void OnDisable()
    {
        KillSpin();
    }

    private void OnDestroy()
    {
        KillSpin();
    }

    private void KillSpin()
    {
        if (spin_tween != null)
        {
            spin_tween.Kill();
            spin_tween = null;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifetime && !has_detonated)
        {
            Detonate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (has_detonated) return;
        if (collision.CompareTag(GameConstants.Tags.Enemy) || collision.CompareTag(GameConstants.Tags.Ground))
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        if (has_detonated) return;
        has_detonated = true;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, stun_radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].CompareTag(GameConstants.Tags.Enemy))
            {
                EnemyBase enemy = colliders[i].GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.ApplyStun(stun_duration);
                }
            }
        }

        if (hit_effect_prefab != null)
        {
            Instantiate(hit_effect_prefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
