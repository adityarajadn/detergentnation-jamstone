using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string title;
    [TextArea(2, 4)]
    [SerializeField] private string content;

    public void SetTooltip(string newTitle, string newContent)
    {
        title = newTitle;
        content = newContent;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UITooltip.instance != null)
        {
            UITooltip.instance.Show(title, content);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltip.instance != null)
        {
            UITooltip.instance.Hide();
        }
    }

    private void OnDisable()
    {
        if (UITooltip.instance != null)
        {
            UITooltip.instance.Hide();
        }
    }
}
