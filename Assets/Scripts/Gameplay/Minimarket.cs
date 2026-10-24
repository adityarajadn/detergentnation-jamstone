using UnityEngine;
using UnityEngine.InputSystem;

public class Minimarket : MonoBehaviour
{
    public static Minimarket instance;

    public Health health;
    public Transform restock_station;
    public float interaction_distance = 2.5f;
    public int repair_cost = 10;
    public int repair_heal_amount = 25;

    private PlayerCombat player_combat;
    private Transform player_transform;

    private void Awake()
    {
        instance = this;
        if (health == null) health = GetComponent<Health>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player_combat = player.GetComponent<PlayerCombat>();
            player_transform = player.transform;
        }
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        if (player_transform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player_combat = player.GetComponent<PlayerCombat>();
                player_transform = player.transform;
            }
            return;
        }

        float dist = Vector2.Distance(player_transform.position, transform.position);
        if (dist <= interaction_distance)
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
            {
                if (player_combat != null)
                {
                    player_combat.Restock();

                    if (health.health_current < health.health_max && player_combat.UseRepairPoints(repair_cost))
                    {
                        health.Heal(repair_heal_amount);
                    }
                }
            }
        }
    }
}
