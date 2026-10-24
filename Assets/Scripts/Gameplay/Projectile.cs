using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 15;
    public float speed = 12f;
    public float lifetime = 3f;
    public bool is_enemy_projectile = false;
    public bool is_floating_bubble = false;
    public float splash_radius = 0f;
    public GameObject hit_effect_prefab;
    public AudioClip hit_sound;

    private Rigidbody2D rb;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (rb != null && !is_floating_bubble)
        {
            rb.linearVelocity = transform.right * speed;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            ExplodeAndDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (is_enemy_projectile)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Minimarket") || collision.CompareTag("Defense"))
            {
                ApplyDamage(collision);
                ExplodeAndDestroy();
            }
        }
        else
        {
            if (collision.CompareTag("Enemy"))
            {
                ApplyDamage(collision);
                ExplodeAndDestroy();
            }
        }
    }

    private void ApplyDamage(Collider2D target)
    {
        if (splash_radius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splash_radius);
            for (int i = 0; i < hits.Length; i++)
            {
                if (is_enemy_projectile && (hits[i].CompareTag("Player") || hits[i].CompareTag("Minimarket") || hits[i].CompareTag("Defense")))
                {
                    Health target_health = hits[i].GetComponent<Health>();
                    if (target_health != null) target_health.TakeDamage(damage);
                }
                else if (!is_enemy_projectile && hits[i].CompareTag("Enemy"))
                {
                    Health target_health = hits[i].GetComponent<Health>();
                    if (target_health != null) target_health.TakeDamage(damage);
                }
            }
        }
        else
        {
            Health health = target.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private void ExplodeAndDestroy()
    {
        if (hit_sound != null)
        {
            AudioManager.PlaySFX(hit_sound);
        }

        if (hit_effect_prefab != null)
        {
            Instantiate(hit_effect_prefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
