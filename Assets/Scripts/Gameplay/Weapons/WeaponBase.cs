using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Configuration")]
    public WeaponData weapon_data;

    [Header("Visual Components")]
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected Transform visual_transform;

    protected virtual void Awake()
    {
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
        if (visual_transform == null && sr != null)
        {
            visual_transform = sr.transform;
        }
    }

    public virtual void Initialize(WeaponData data)
    {
        weapon_data = data;
    }

    public virtual void OnAttack() { }
    public virtual void OnReload() { }
}
