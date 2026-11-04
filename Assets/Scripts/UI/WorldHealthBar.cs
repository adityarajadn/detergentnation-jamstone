using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label_text;

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
}
