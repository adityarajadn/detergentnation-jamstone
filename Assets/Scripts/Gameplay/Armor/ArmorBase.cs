using UnityEngine;

public class ArmorBase : MonoBehaviour
{
    [Header("Armor Configuration")]
    public string armor_name = "Armor";
    public int armor_rating = 1;

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public virtual int MitigateDamage(int incoming_damage)
    {
        return Mathf.Max(1, incoming_damage - armor_rating);
    }

    public virtual void OnDamageTaken(int final_damage, Transform player_transform) { }
}
