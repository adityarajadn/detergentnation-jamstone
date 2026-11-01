using UnityEngine;

public class DefenseRollingBarrel : DefenseObject
{
    [SerializeField] private float roll_speed = 9f;
    [SerializeField] private GameObject explosion_effect_prefab;

    private bool is_rolling = false;
    private float roll_direction = 1f;

    protected override bool CanInteract()
    {
        return !is_rolling;
    }

    protected override string GetInteractionPrompt(string keyName)
    {
        return "[" + keyName + "] ROLL BARREL";
    }

    protected override void OnInteract()
    {
        ReleaseBarrel();
    }

    public void ReleaseBarrel()
    {
        if (is_rolling) return;
        is_rolling = true;
        AudioManager.PlaySFX("barrel_roll");
        roll_direction = transform.position.x < 0 ? -1f : 1f;
        HidePrompt();
        Destroy(gameObject, 6f);
    }

    protected override void Update()
    {
        if (is_rolling)
        {
            transform.Translate(Vector3.right * (roll_direction * roll_speed * Time.deltaTime));
            if (visuals_container != null)
            {
                visuals_container.transform.Rotate(0f, 0f, -roll_direction * 360f * Time.deltaTime);
            }
            return;
        }

        base.Update();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (is_rolling && collision.CompareTag(GameConstants.Tags.Enemy))
        {
            ExplodeBarrel();
        }
    }

    private void ExplodeBarrel()
    {
        AudioManager.PlaySFX("barrel_break");

        if (explosion_effect_prefab != null)
        {
            Instantiate(explosion_effect_prefab, transform.position, Quaternion.identity);
        }

        float radius = 3.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag(GameConstants.Tags.Enemy))
            {
                Health enemy_health = hits[i].GetComponent<Health>();
                if (enemy_health != null && !enemy_health.is_dead)
                {
                    enemy_health.TakeDamage(defense_data != null ? defense_data.damage : 80);
                }
            }
        }
        OnDestroyed();
    }

    protected override void PlayBreakAudio()
    {
        if (!is_rolling)
        {
            AudioManager.PlaySFX("barrel_break");
        }
    }
}
