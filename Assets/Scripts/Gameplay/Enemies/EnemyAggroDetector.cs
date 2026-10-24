using UnityEngine;

public class EnemyAggroDetector : MonoBehaviour
{
    private EnemyBase enemy_base;

    private void Awake()
    {
        enemy_base = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemy_base != null && collision.CompareTag(GameConstants.Tags.Player))
        {
            enemy_base.OnPlayerEnteredAggro(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enemy_base != null && collision.CompareTag(GameConstants.Tags.Player))
        {
            enemy_base.OnPlayerExitedAggro(collision.transform);
        }
    }
}
