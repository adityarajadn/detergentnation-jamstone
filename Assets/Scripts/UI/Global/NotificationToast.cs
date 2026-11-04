using DG.Tweening;
using TMPro;
using UnityEngine;

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
    [SerializeField] private TMP_Text message_text;

    [Header("Settings")]
    [SerializeField] private float slide_offset_y = 35f;
    [SerializeField] private float anim_duration = 0.2f;
    [SerializeField] private float display_duration = 1.8f;

    private Sequence toast_sequence;
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

    private void OnDisable()
    {
        KillToast();
    }

    private void OnDestroy()
    {
        KillToast();
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

        PlayToast();
    }

    private void PlayToast()
    {
        if (container == null || canvas_group == null) return;

        KillToast();

        Vector2 start_pos = base_anchored_pos + new Vector2(0f, slide_offset_y);
        Vector2 target_pos = base_anchored_pos;

        container.anchoredPosition = start_pos;
        canvas_group.alpha = 0f;
        canvas_group.blocksRaycasts = false;

        toast_sequence = DOTween.Sequence().SetUpdate(true);

        // Slide In + Fade In
        toast_sequence.Append(container.DOAnchorPos(target_pos, anim_duration).SetEase(Ease.InOutSine));
        toast_sequence.Join(canvas_group.DOFade(1f, anim_duration).SetEase(Ease.InOutSine));

        // Display hold
        toast_sequence.AppendInterval(display_duration);

        // Slide Out + Fade Out
        toast_sequence.Append(container.DOAnchorPos(start_pos, anim_duration).SetEase(Ease.InOutSine));
        toast_sequence.Join(canvas_group.DOFade(0f, anim_duration).SetEase(Ease.InOutSine));

        toast_sequence.OnComplete(() => toast_sequence = null);
    }

    private void KillToast()
    {
        if (toast_sequence != null)
        {
            toast_sequence.Kill();
            toast_sequence = null;
        }
    }
}
