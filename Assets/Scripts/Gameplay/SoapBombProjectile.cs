using UnityEngine;

public class SoapBombProjectile : MonoBehaviour
{
    public int damage = 70;
    public float explosion_radius = 3.0f;
    public float lifetime = 4.0f;
    public GameObject explosion_effect_prefab;

    private Rigidbody2D rb;
    private float timer = 0f;
    private bool has_exploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 velocity)
    {
        if (rb != null)
        {
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
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ground"))
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (has_exploded) return;
        has_exploded = true;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosion_radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].CompareTag("Enemy"))
            {
                Health health = colliders[i].GetComponent<Health>();
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
}
