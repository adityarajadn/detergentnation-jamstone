using DG.Tweening;
using UnityEngine;

public class DefenseRollingBarrel : DefenseObject
{
    private const float RollLifetimeSeconds = 6f;

    [SerializeField] private float roll_speed = 9f;
    [SerializeField] private GameObject explosion_effect_prefab;

    private bool is_rolling = false;
    private float roll_direction = 1f;
    private Tween move_tween;
    private Tween spin_tween;

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
        AudioManager.PlaySFX(GameConstants.AudioKeys.BarrelRoll);
        roll_direction = transform.position.x < 0 ? -1f : 1f;
        HidePrompt();
        StartRolling();
        Destroy(gameObject, RollLifetimeSeconds);
    }

    private void StartRolling()
    {
        KillTweens();

        // Linear travel along X for the barrel's whole lifetime
        float travel = roll_direction * roll_speed * RollLifetimeSeconds;
        move_tween = transform.DOMoveX(transform.position.x + travel, RollLifetimeSeconds).SetEase(Ease.Linear);

        // One full turn per second, looping
        if (visuals_container != null)
        {
            spin_tween = visuals_container.transform
                .DOLocalRotate(new Vector3(0f, 0f, -roll_direction * 360f), 1f, RotateMode.FastBeyond360)
                .SetRelative()
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    private void KillTweens()
    {
        if (move_tween != null) { move_tween.Kill(); move_tween = null; }
        if (spin_tween != null) { spin_tween.Kill(); spin_tween = null; }
    }

    private void OnDisable()
    {
        KillTweens();
    }

    protected override void OnDestroy()
    {
        KillTweens();
        base.OnDestroy();
    }

    protected override void Update()
    {
        if (is_rolling) return;

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
        AudioManager.PlaySFX(GameConstants.AudioKeys.BarrelBreak);

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
            AudioManager.PlaySFX(GameConstants.AudioKeys.BarrelBreak);
        }
    }
}
