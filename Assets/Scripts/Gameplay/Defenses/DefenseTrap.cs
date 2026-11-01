using UnityEngine;

public class DefenseTrap : DefenseObject
{
    protected override bool CanInteract() => false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameConstants.Tags.Enemy))
        {
            TriggerTrap();
        }
    }

    public void TriggerTrap()
    {
        AudioManager.PlaySFX("balloon_pop");
        float radius = 2.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag(GameConstants.Tags.Enemy))
            {
                Health enemy_health = hits[i].GetComponent<Health>();
                if (enemy_health != null && !enemy_health.is_dead)
                {
                    enemy_health.TakeDamage(defense_data != null ? defense_data.damage : 60);
                }
            }
        }
        OnDestroyed();
    }
}
