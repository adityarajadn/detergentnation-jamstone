using UnityEngine;
using DG.Tweening;

public class SoapBombProjectile : MonoBehaviour
{
    public int damage = 70;
    public float explosion_radius = 3.0f;
    public float lifetime = 4.0f;
    public GameObject explosion_effect_prefab;

    private Rigidbody2D rb;
    private float timer = 0f;
    private bool has_exploded = false;
    private Tween move_tween;
    private Tween rotate_tween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void LaunchArc(Vector3 target_pos, float duration = 0.55f, float jump_power = 2.5f)
    {
        if (rb != null)
        {
            rb.simulated = false;
        }

        move_tween = transform.DOJump(target_pos, jump_power, 1, duration)
            .SetEase(Ease.Linear)
            .OnComplete(Explode);

        rotate_tween = transform.DORotate(new Vector3(0, 0, -720f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }

    public void Launch(Vector2 velocity)
    {
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = velocity;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime && !has_exploded)
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!has_exploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (has_exploded) return;
        if (collision.isTrigger || collision.GetComponent<EnemyAggroDetector>() != null) return;

        if (collision.CompareTag(GameConstants.Tags.Enemy) || collision.CompareTag(GameConstants.Tags.Ground))
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (has_exploded) return;
        has_exploded = true;

        if (move_tween != null && move_tween.IsActive()) move_tween.Kill();
        if (rotate_tween != null && rotate_tween.IsActive()) rotate_tween.Kill();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosion_radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && !colliders[i].isTrigger && colliders[i].CompareTag(GameConstants.Tags.Enemy))
            {
                Health health = colliders[i].GetComponentInParent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }

        if (explosion_effect_prefab != null)
        {
            Instantiate(explosion_effect_prefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (move_tween != null && move_tween.IsActive()) move_tween.Kill();
        if (rotate_tween != null && rotate_tween.IsActive()) rotate_tween.Kill();
    }
}
