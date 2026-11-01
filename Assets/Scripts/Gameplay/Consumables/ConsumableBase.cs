using UnityEngine;

public abstract class ConsumableBase : MonoBehaviour
{
    [Header("Consumable Info")]
    public string consumable_name;
    public int price;

    [Header("Popup Info")]
    [SerializeField] protected string popup_label = "USED!";
    [SerializeField] protected Color popup_color = Color.white;

    public string PopupLabel => !string.IsNullOrEmpty(popup_label) ? popup_label : consumable_name.ToUpper();
    public Color PopupColor => popup_color;

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public abstract bool Use(PlayerCombat combat, Vector3 target_world_pos);
}
