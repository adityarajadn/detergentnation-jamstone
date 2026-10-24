using UnityEngine;

public class EnemyAggroDetector : MonoBehaviour
{
    private EnemyController enemy_controller;

    private void Awake()
    {
        enemy_controller = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemy_controller != null && collision.CompareTag("Player"))
        {
            enemy_controller.OnPlayerEnteredAggro(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enemy_controller != null && collision.CompareTag("Player"))
        {
            enemy_controller.OnPlayerExitedAggro(collision.transform);
        }
    }
}
