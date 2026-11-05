using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    [Header("Components & Anchors")]
    [SerializeField] private Transform visuals;

    [Header("Visual Settings")]
    [SerializeField] private bool enable_pulse = true;
    [SerializeField] private float pulse_speed = 2.5f;
    [SerializeField] private float pulse_scale_amount = 0.08f;

    [Header("Events")]
    public UnityEvent<GameObject> on_entity_entered;

    private Vector3 base_visual_scale = Vector3.one;
    private Tween pulse_tween;

    private void Awake()
    {
        if (visuals == null)
        {
            visuals = transform.Find("Visuals");
        }

        if (visuals != null)
        {
            base_visual_scale = visuals.localScale;
        }
    }

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        KillPulse();
    }

    private void OnDestroy()
    {
        KillPulse();
    }

    private void StartPulse()
    {
        KillPulse();
        if (!enable_pulse || visuals == null) return;

        // Smooth back-and-forth scale between (1 - amount) and (1 + amount); one half cycle = PI / speed
        visuals.localScale = base_visual_scale * (1f - pulse_scale_amount);
        pulse_tween = visuals
            .DOScale(base_visual_scale * (1f + pulse_scale_amount), Mathf.PI / Mathf.Max(pulse_speed, 0.01f))
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void KillPulse()
    {
        if (pulse_tween != null)
        {
            pulse_tween.Kill();
            pulse_tween = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        on_entity_entered?.Invoke(collision.gameObject);
    }
}
