using UnityEngine;

public class ArmorIAmNumberOne : ArmorBase
{
    [SerializeField] private GameObject bubble_prefab;
    [SerializeField] private int bubble_count = 3;

    private void Reset()
    {
        armor_name = GameConstants.EquipmentNames.ArmorIAmNumberOne;
        armor_rating = 2;
    }

    protected override void Awake()
    {
        base.Awake();
        armor_name = GameConstants.EquipmentNames.ArmorIAmNumberOne;
        armor_rating = 2;
    }

    public override void OnDamageTaken(int final_damage, Transform player_transform)
    {
        if (bubble_prefab == null) return;
        Vector3 origin = player_transform != null ? player_transform.position : transform.position;

        for (int i = 0; i < bubble_count; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * 0.4f;
            GameObject bubble = Instantiate(bubble_prefab, origin + offset, Quaternion.identity);
            Projectile p = bubble.GetComponent<Projectile>();
            if (p != null)
            {
                p.damage = 10;
                p.is_floating_bubble = true;
                p.lifetime = 12f;
                p.is_enemy_projectile = false;
            }
        }
    }
}
