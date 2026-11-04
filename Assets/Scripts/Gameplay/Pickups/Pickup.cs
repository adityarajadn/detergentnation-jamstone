using UnityEngine;

public enum PickupType
{
    Candy,
    HealthPoint,
    RepairPoint
}

public class Pickup : MonoBehaviour
{
    public PickupType pickup_type;
    public int amount = 1;
    public float magnet_speed = 6f;
    public AudioClip pickup_sound;

    private Transform player_transform;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            player_transform = PlayerController.Instance.transform;
        }
    }

    private void Update()
    {
        if (player_transform == null)
        {
            if (PlayerController.Instance != null)
            {
                player_transform = PlayerController.Instance.transform;
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player_transform.position);
        if (dist < 3f)
        {
            transform.position = Vector2.MoveTowards(transform.position, player_transform.position, magnet_speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameConstants.Tags.Player))
        {
            if (pickup_sound != null)
            {
                AudioManager.PlaySFX(pickup_sound);
            }

            if (pickup_type == PickupType.Candy)
            {
                if (GameManager.instance != null)
                {
                    GameManager.instance.AddCandy(amount);
                }
                if (GameplayUI.Instance != null)
                {
                    GameplayUI.Instance.ShowPickupFeedback("+" + amount + " CANDY");
                }
            }
            else if (pickup_type == PickupType.HealthPoint)
            {
                Health player_health = collision.GetComponent<Health>();
                if (player_health != null)
                {
                    player_health.Heal(amount);
                }
                if (GameplayUI.Instance != null)
                {
                    GameplayUI.Instance.ShowPickupFeedback("+" + amount + " HP");
                }
            }
            else if (pickup_type == PickupType.RepairPoint)
            {
                PlayerCombat player_combat = collision.GetComponent<PlayerCombat>();
                if (player_combat != null)
                {
                    player_combat.AddRepairPoints(amount);
                }
                if (GameplayUI.Instance != null)
                {
                    GameplayUI.Instance.ShowPickupFeedback("+" + amount + " REPAIR POINT");
                }
            }

            Destroy(gameObject);
        }
    }
}
