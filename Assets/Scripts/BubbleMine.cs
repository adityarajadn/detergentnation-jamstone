using UnityEngine;

public class BubbleMine : MonoBehaviour
{
    public int damage = 35;
    public float lifetime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage(damage, transform.position);
            AudioManager.Instance?.PlaySfx("hit");
            Destroy(gameObject);
        }
    }
}
