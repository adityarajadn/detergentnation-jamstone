using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 7.0f;
    public Vector2 direction = Vector2.left;
    public int damage = 10;
    public float lifetime = 6.0f;

    public void Setup(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.TryGetComponent<Player>(out var player))
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (col.CompareTag("Minimarket"))
        {
            if (col.TryGetComponent<Minimarket>(out var store))
            {
                store.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
