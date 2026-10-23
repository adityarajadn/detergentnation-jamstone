using UnityEngine;

public class SoapBomb : MonoBehaviour
{
    public int damage = 80;
    public float radius = 2.5f;
    public float fuse = 2.0f;
    public LayerMask enemyLayer;

    private void Start()
    {
        Invoke(nameof(Explode), fuse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            CancelInvoke(nameof(Explode));
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(damage, transform.position);
            }
        }
        AudioManager.Instance?.PlaySfx("alarm");
        Destroy(gameObject);
    }
}
