using UnityEngine;

public abstract class ConsumableBase : MonoBehaviour
{
    [Header("Consumable Info")]
    public string consumable_name;

    [Header("Popup Info")]
    [SerializeField] protected string popup_label = "USED!";

    public string PopupLabel => !string.IsNullOrEmpty(popup_label) ? popup_label : consumable_name.ToUpper();

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public abstract bool Use(PlayerCombat combat, Vector3 target_world_pos);
}
