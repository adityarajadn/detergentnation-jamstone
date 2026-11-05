using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITooltip : MonoBehaviour
{
    public static UITooltip instance { get; private set; }

    [SerializeField] private CanvasGroup canvas_group;
    [SerializeField] private RectTransform tooltip_rect;
    [SerializeField] private TMP_Text title_text;
    [SerializeField] private TMP_Text content_text;
    [SerializeField] private Vector2 cursor_offset = new Vector2(16f, -16f);

    private Canvas parent_canvas;
    private RectTransform canvas_rect;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (canvas_group == null) canvas_group = GetComponent<CanvasGroup>();
        if (tooltip_rect == null) tooltip_rect = GetComponent<RectTransform>();

        parent_canvas = GetComponentInParent<Canvas>();
        if (parent_canvas != null)
        {
            canvas_rect = parent_canvas.GetComponent<RectTransform>();
        }

        Hide();
    }

    private void Update()
    {
        if (canvas_group == null || canvas_group.alpha <= 0f) return;
        FollowCursor();
    }

    public void Show(string title, string content)
    {
        if (string.IsNullOrEmpty(content) && string.IsNullOrEmpty(title))
        {
            Hide();
            return;
        }

        if (title_text != null)
        {
            title_text.gameObject.SetActive(!string.IsNullOrEmpty(title));
            title_text.text = title;
        }

        if (content_text != null)
        {
            content_text.gameObject.SetActive(!string.IsNullOrEmpty(content));
            content_text.text = content;
        }

        if (canvas_group != null)
        {
            canvas_group.alpha = 1f;
            canvas_group.blocksRaycasts = false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip_rect);
        FollowCursor();
    }

    public void Hide()
    {
        if (canvas_group != null)
        {
            canvas_group.alpha = 0f;
            canvas_group.blocksRaycasts = false;
        }
    }

    private void FollowCursor()
    {
        if (parent_canvas == null || canvas_rect == null) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas_rect, mousePos, parent_canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parent_canvas.worldCamera, out localPoint);

        localPoint += cursor_offset;

        // Clamp to screen edges
        Vector2 min = canvas_rect.rect.min;
        Vector2 max = canvas_rect.rect.max;
        Vector2 size = tooltip_rect.sizeDelta;

        if (localPoint.x + size.x > max.x)
        {
            localPoint.x = mousePos.x > Screen.width * 0.5f ? localPoint.x - size.x - (cursor_offset.x * 2f) : max.x - size.x;
        }

        if (localPoint.y - size.y < min.y)
        {
            localPoint.y = min.y + size.y;
        }

        tooltip_rect.localPosition = localPoint;
    }
}
