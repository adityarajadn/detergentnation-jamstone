using System.Collections;
using UnityEngine;

public class DefenseTrap : DefenseObject
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject pop_effect_prefab;

    private bool is_triggered = false;

    protected override void Awake()
    {
        base.Awake();
        if (animator == null && visuals != null)
        {
            animator = visuals.GetComponent<Animator>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    protected override bool CanInteract() => false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!is_triggered && collision.CompareTag(GameConstants.Tags.Enemy))
        {
            TriggerTrap();
        }
    }

    public void TriggerTrap()
    {
        if (is_triggered) return;
        is_triggered = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        AudioManager.PlaySFX(GameConstants.AudioKeys.BalloonPop);

        if (pop_effect_prefab != null)
        {
            Instantiate(pop_effect_prefab, transform.position, Quaternion.identity);
        }

        float radius = defense_data != null ? defense_data.attack_range : 2.5f;
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

        if (animator != null)
        {
            animator.SetTrigger("explode");
            StartCoroutine(ExplodeRoutine());
        }
        else
        {
            OnDestroyed();
        }
    }

    private IEnumerator ExplodeRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        OnDestroyed();
    }
}
