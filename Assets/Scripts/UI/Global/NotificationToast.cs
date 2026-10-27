using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ToastType
{
    Success,
    Warning,
    Info
}

public class NotificationToast : MonoBehaviour
{
    public static NotificationToast instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup canvas_group;
    [SerializeField] private Image background_image;
    [SerializeField] private Image accent_bar;
    [SerializeField] private TMP_Text message_text;

    [Header("Settings")]
    [SerializeField] private float slide_offset_y = 35f;
    [SerializeField] private float anim_duration = 0.2f;
    [SerializeField] private float display_duration = 1.8f;

    [Header("Color Schemes")]
    [SerializeField] private Color success_color = new Color(0.20f, 0.75f, 0.45f, 1f);
    [SerializeField] private Color warning_color = new Color(0.92f, 0.32f, 0.32f, 1f);
    [SerializeField] private Color info_color = new Color(0.25f, 0.65f, 0.95f, 1f);

    private Coroutine active_toast_routine;
    private Vector2 base_anchored_pos;

    private void Awake()
    {
        instance = this;

        if (container == null) container = GetComponent<RectTransform>();
        if (canvas_group == null) canvas_group = GetComponent<CanvasGroup>();

        if (container != null)
        {
            base_anchored_pos = container.anchoredPosition;
        }

        if (canvas_group != null)
        {
            canvas_group.alpha = 0f;
            canvas_group.blocksRaycasts = false;
        }
    }

    public static void Show(string message, ToastType type = ToastType.Info)
    {
        GlobalUIManager.ShowToast(message, type);
    }

    public void TriggerToast(string message, ToastType type)
    {
        if (message_text != null)
        {
            message_text.text = message;
        }

        Color target_accent = info_color;
        switch (type)
        {
            case ToastType.Success:
                target_accent = success_color;
                break;
            case ToastType.Warning:
                target_accent = warning_color;
                break;
            case ToastType.Info:
                target_accent = info_color;
                break;
        }

        if (accent_bar != null)
        {
            accent_bar.color = target_accent;
        }

        if (active_toast_routine != null)
        {
            StopCoroutine(active_toast_routine);
        }
        active_toast_routine = StartCoroutine(ToastRoutine());
    }

    private IEnumerator ToastRoutine()
    {
        if (container == null || canvas_group == null) yield break;

        Vector2 startPos = base_anchored_pos + new Vector2(0f, slide_offset_y);
        Vector2 targetPos = base_anchored_pos;

        container.anchoredPosition = startPos;
        canvas_group.alpha = 0f;
        canvas_group.blocksRaycasts = false;

        // Slide In + Fade In
        float elapsed = 0f;
        while (elapsed < anim_duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / anim_duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            container.anchoredPosition = Vector2.Lerp(startPos, targetPos, smooth);
            canvas_group.alpha = smooth;
            yield return null;
        }

        container.anchoredPosition = targetPos;
        canvas_group.alpha = 1f;

        // Display hold
        yield return new WaitForSecondsRealtime(display_duration);

        // Slide Out + Fade Out
        elapsed = 0f;
        while (elapsed < anim_duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / anim_duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            container.anchoredPosition = Vector2.Lerp(targetPos, startPos, smooth);
            canvas_group.alpha = 1f - smooth;
            yield return null;
        }

        container.anchoredPosition = startPos;
        canvas_group.alpha = 0f;
        active_toast_routine = null;
    }
}
