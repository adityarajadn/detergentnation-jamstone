using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SoapBarrel : MonoBehaviour
{
    public float rollSpeed = 6.0f;
    public int damage = 60;
    public float lifetime = 10.0f;
    public Vector2 rollDirection = Vector2.right;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
        rb.linearVelocity = rollDirection * rollSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(damage, transform.position);
            }
            AudioManager.Instance?.PlaySfx("hit");
            Destroy(gameObject);
        }
    }
}
