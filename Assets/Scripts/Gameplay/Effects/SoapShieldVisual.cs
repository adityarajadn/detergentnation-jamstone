using DG.Tweening;
using UnityEngine;

public class SoapShieldVisual : MonoBehaviour
{
    [SerializeField] private Health target_health;
    [SerializeField] private SpriteRenderer sprite_renderer;
    [SerializeField] private Vector3 base_scale = new Vector3(1.3f, 1.3f, 1f);
    [SerializeField] private float pulse_scale_multiplier = 1.08f;
    [SerializeField] private float pulse_duration = 0.8f;

    private Tween pulse_tween;
    private Tween hit_tween;

    private void Awake()
    {
        if (sprite_renderer == null)
        {
            sprite_renderer = GetComponent<SpriteRenderer>();
        }

        if (target_health == null)
        {
            target_health = GetComponentInParent<Health>();
        }
    }

    private void Start()
    {
        UpdateVisual(target_health != null ? target_health.shield_charges : 0, false);
    }

    private void OnEnable()
    {
        if (target_health != null)
        {
            target_health.OnShieldChargesChanged += HandleShieldChanged;
            UpdateVisual(target_health.shield_charges, false);
        }
    }

    private void OnDisable()
    {
        if (target_health != null)
        {
            target_health.OnShieldChargesChanged -= HandleShieldChanged;
        }

        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void HandleShieldChanged(int charges)
    {
        UpdateVisual(charges, true);
    }

    private void UpdateVisual(int charges, bool animate_transition)
    {
        if (charges > 0)
        {
            if (sprite_renderer != null)
            {
                sprite_renderer.enabled = true;
            }

            KillTweens();

            if (animate_transition)
            {
                transform.localScale = base_scale;
                hit_tween = transform.DOPunchScale(base_scale * 0.25f, 0.25f, 5, 0.5f).OnComplete(StartPulseTween);
                AudioManager.PlaySFX(GameConstants.AudioKeys.BalloonInflate);
            }
            else
            {
                transform.localScale = base_scale;
                StartPulseTween();
            }
        }
        else
        {
            KillTweens();
            if (animate_transition && gameObject.activeInHierarchy && sprite_renderer != null && sprite_renderer.enabled)
            {
                AudioManager.PlaySFX(GameConstants.AudioKeys.BalloonPop);

                hit_tween = transform.DOScale(Vector3.zero, 0.15f).OnComplete(() =>
                {
                    if (sprite_renderer != null)
                    {
                        sprite_renderer.enabled = false;
                    }
                    transform.localScale = base_scale;
                });
            }
            else
            {
                if (sprite_renderer != null)
                {
                    sprite_renderer.enabled = false;
                }
                transform.localScale = base_scale;
            }
        }
    }

    private void StartPulseTween()
    {
        if (!gameObject.activeInHierarchy || sprite_renderer == null || !sprite_renderer.enabled) return;

        pulse_tween = transform.DOScale(base_scale * pulse_scale_multiplier, pulse_duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void KillTweens()
    {
        if (pulse_tween != null)
        {
            pulse_tween.Kill();
            pulse_tween = null;
        }

        if (hit_tween != null)
        {
            hit_tween.Kill();
            hit_tween = null;
        }

        transform.DOKill();
    }
}
