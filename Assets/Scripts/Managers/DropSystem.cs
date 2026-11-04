using UnityEngine;

public class DropSystem : MonoBehaviour
{
    public static DropSystem instance;

    [Header("Drop Prefabs")]
    [SerializeField] private GameObject candy_prefab;
    [SerializeField] private GameObject health_point_prefab;
    [SerializeField] private GameObject repair_point_prefab;

    [Header("Drop Rates")]
    [Range(0f, 1f)] [SerializeField] private float candy_drop_chance = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float health_drop_chance = 0.15f;
    [Range(0f, 1f)] [SerializeField] private float repair_drop_chance = 0.15f;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void SpawnDrops(Vector3 position, EnemyData enemy_data)
    {
        float roll_candy = Random.value;
        if (roll_candy <= candy_drop_chance && candy_prefab != null)
        {
            int candy_amount = Random.Range(enemy_data.candy_reward_min, enemy_data.candy_reward_max + 1);
            GameObject obj = Instantiate(candy_prefab, position, Quaternion.identity);
            Pickup p = obj.GetComponent<Pickup>();
            if (p != null) p.amount = candy_amount;
        }

        float roll_other = Random.value;
        if (roll_other < health_drop_chance && health_point_prefab != null)
        {
            Instantiate(health_point_prefab, position + Vector3.up * 0.3f, Quaternion.identity);
        }
        else if (roll_other < (health_drop_chance + repair_drop_chance) && repair_point_prefab != null)
        {
            Instantiate(repair_point_prefab, position + Vector3.up * 0.3f, Quaternion.identity);
        }
    }
}
