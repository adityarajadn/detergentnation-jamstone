using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label_text;

    [Header("Fill Sprites (optional)")]
    [SerializeField] private Image fill_image;
    [SerializeField] private Sprite fill_sprite_high;
    [SerializeField] private Sprite fill_sprite_mid;
    [SerializeField] private Sprite fill_sprite_low;
    [SerializeField, Range(0f, 1f)] private float mid_threshold = 0.6f;
    [SerializeField, Range(0f, 1f)] private float low_threshold = 0.3f;

    [Header("Settings")]
    [SerializeField] private bool hide_when_full = false;
    [SerializeField] private bool show_numeric_text = false;

    private Health target_health;

    private void Awake()
    {
        if (slider == null) slider = GetComponentInChildren<Slider>();
        if (label_text == null) label_text = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        ResolveTargetHealth();
    }

    private void OnEnable()
    {
        if (target_health != null)
        {
            target_health.on_health_changed.AddListener(OnHealthChanged);
            UpdateBar(target_health.health_current, target_health.health_max);
        }
    }

    private void OnDisable()
    {
        if (target_health != null)
        {
            target_health.on_health_changed.RemoveListener(OnHealthChanged);
        }
    }

    public void ResolveTargetHealth()
    {
        if (target_health == null)
        {
            target_health = GetComponentInParent<Health>();
        }

        if (target_health != null)
        {
            target_health.on_health_changed.RemoveListener(OnHealthChanged);
            target_health.on_health_changed.AddListener(OnHealthChanged);
            UpdateBar(target_health.health_current, target_health.health_max);
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateBar(current, max);
    }

    public void UpdateBar(int current, int max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;
        }

        UpdateFillSprite(current, max);

        if (label_text != null && show_numeric_text)
        {
            label_text.text = current + " / " + max;
        }

        if (hide_when_full)
        {
            bool is_full = current >= max;
            gameObject.SetActive(!is_full);
        }
    }

    private void UpdateFillSprite(int current, int max)
    {
        if (fill_image == null || max <= 0) return;

        float ratio = (float)current / max;
        Sprite next = ratio > mid_threshold ? fill_sprite_high
            : ratio > low_threshold ? fill_sprite_mid
            : fill_sprite_low;

        if (next != null) fill_image.sprite = next;
    }
}
