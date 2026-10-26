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
        ResolvePlayerReferences();
        SetupInteractable();
    }

    private void SetupInteractable()
    {
        Interactable interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<Interactable>();
        }
        interactable.SetActionLabel("REPAIR & RESTOCK");
        interactable.SetInteractionRadius(interaction_distance);
        interactable.SetPromptOffset(new Vector3(0f, 2.5f, 0f));
        interactable.on_interacted.AddListener(OnInteract);
    }

    public void OnInteract(PlayerCombat combat)
    {
        if (combat == null && player_combat != null) combat = player_combat;
        if (combat == null) return;

        combat.Restock();

        if (health != null && health.health_current < health.health_max && combat.UseRepairPoints(repair_cost))
        {
            health.Heal(repair_heal_amount);
        }
    }

    private void ResolvePlayerReferences()
    {
        if (PlayerCombat.Instance != null)
        {
            player_combat = PlayerCombat.Instance;
            player_transform = PlayerCombat.Instance.transform;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
        if (player != null)
        {
            player_combat = player.GetComponent<PlayerCombat>();
            player_transform = player.transform;
        }
    }

    private void Update()
    {
    }
}
