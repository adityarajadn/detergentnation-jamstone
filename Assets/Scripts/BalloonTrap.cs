using UnityEngine;

public class BalloonTrap : MonoBehaviour
{
    public int damage = 90;
    public float explosionRadius = 2.5f;
    public LayerMask enemyLayer;

    private bool exploded = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (exploded) return;
        if (col.CompareTag("Enemy") || col.CompareTag("PlayerProjectile"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        exploded = true;
        AudioManager.Instance?.PlaySfx("alarm");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(damage, transform.position);
            }
        }

        Destroy(gameObject);
    }
}
