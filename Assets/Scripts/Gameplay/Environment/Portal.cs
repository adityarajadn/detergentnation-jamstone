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

    private void Update()
    {
        if (enable_pulse && visuals != null)
        {
            float wave = Mathf.Sin(Time.time * pulse_speed) * pulse_scale_amount;
            visuals.localScale = base_visual_scale * (1f + wave);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        on_entity_entered?.Invoke(collision.gameObject);
    }
}
