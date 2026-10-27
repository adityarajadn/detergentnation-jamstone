using UnityEngine;
using UnityEngine.InputSystem;

public class Minimarket : MonoBehaviour
{
    public static Minimarket instance;

    public Health health;
    public Transform restock_station;
    public float interaction_distance = 3.2f;
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

        bool restocked = false;
        PlayerAmmo ammo = combat.GetComponent<PlayerAmmo>();
        if (ammo != null && ammo.ammo_carried < ammo.ammo_carried_max)
        {
            combat.Restock();
            if (LevelManager.instance != null) LevelManager.instance.RegisterRestock();
            restocked = true;
        }

        bool repaired = false;
        if (health != null && health.health_current < health.health_max)
        {
            if (combat.repair_points >= repair_cost)
            {
                if (combat.UseRepairPoints(repair_cost))
                {
                    health.Heal(repair_heal_amount);
                    repaired = true;
                }
            }
            else
            {
                NotificationToast.Show("Need " + repair_cost + " repair points to fix Minimarket! (" + combat.repair_points + "/" + repair_cost + ")", ToastType.Warning);
            }
        }

        if (repaired && restocked)
        {
            NotificationToast.Show("Minimarket repaired (+" + repair_heal_amount + " HP) & Ammo restocked!", ToastType.Success);
        }
        else if (repaired)
        {
            NotificationToast.Show("Minimarket repaired! +" + repair_heal_amount + " HP", ToastType.Success);
        }
        else if (restocked)
        {
            NotificationToast.Show("Ammo restocked to MAX!", ToastType.Success);
        }
        else if (health != null && health.health_current >= health.health_max && (ammo == null || ammo.ammo_carried >= ammo.ammo_carried_max))
        {
            NotificationToast.Show("Minimarket is full health & ammo is already full!", ToastType.Info);
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
