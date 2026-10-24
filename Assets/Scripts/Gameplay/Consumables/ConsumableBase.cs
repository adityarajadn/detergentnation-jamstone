using UnityEngine;

public abstract class ConsumableBase : MonoBehaviour
{
    [Header("Consumable Info")]
    public string consumable_name;
    public int price;

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public abstract bool Use(PlayerCombat combat, Vector3 target_world_pos);
}
