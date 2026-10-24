using UnityEngine;

public class DropSystem : MonoBehaviour
{
    public static DropSystem instance;

    [SerializeField] private GameObject candy_prefab;
    [SerializeField] private GameObject health_point_prefab;
    [SerializeField] private GameObject repair_point_prefab;

    private void Awake()
    {
        instance = this;
    }

    public void SpawnDrops(Vector3 position, EnemyData enemy_data)
    {
        float roll_candy = Random.value;
        if (roll_candy <= 0.7f && candy_prefab != null)
        {
            int candy_amount = Random.Range(enemy_data.candy_reward_min, enemy_data.candy_reward_max + 1);
            GameObject obj = Instantiate(candy_prefab, position, Quaternion.identity);
            Pickup p = obj.GetComponent<Pickup>();
            if (p != null) p.amount = candy_amount;
        }

        float roll_other = Random.value;
        if (roll_other < 0.15f && health_point_prefab != null)
        {
            Instantiate(health_point_prefab, position + Vector3.up * 0.3f, Quaternion.identity);
        }
        else if (roll_other < 0.30f && repair_point_prefab != null)
        {
            Instantiate(repair_point_prefab, position + Vector3.up * 0.3f, Quaternion.identity);
        }
    }
}
