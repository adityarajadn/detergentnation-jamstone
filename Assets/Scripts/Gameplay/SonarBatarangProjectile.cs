using UnityEngine;

public class SonarBatarangProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float stun_radius = 4.0f;
    public float stun_duration = 4.0f;
    public float lifetime = 3.0f;
    public GameObject hit_effect_prefab;

    private Rigidbody2D rb;
    private float timer = 0f;
    private bool has_detonated = false;

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

    private void Update()
    {
        timer += Time.deltaTime;
        transform.Rotate(0, 0, 720f * Time.deltaTime);

        if (timer >= lifetime && !has_detonated)
        {
            Detonate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (has_detonated) return;
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ground"))
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
            if (colliders[i] != null && colliders[i].CompareTag("Enemy"))
            {
                EnemyController enemy = colliders[i].GetComponent<EnemyController>();
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
