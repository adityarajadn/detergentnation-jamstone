using UnityEngine;

public class SoapBullet : MonoBehaviour
{
    public float speed = 12f;
    public Vector2 direction = Vector2.right;
    public int damage = 25;
    public float maxRange = 15f;
    private float traveled = 0f;

    private void Update()
    {
        float dist = speed * Time.deltaTime;
        transform.position += (Vector3)(direction * dist);
        traveled += dist;
        if (traveled >= maxRange) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage(damage, transform.position);
            AudioManager.Instance?.PlaySfx("hit");
            Destroy(gameObject);
        }
        else if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
