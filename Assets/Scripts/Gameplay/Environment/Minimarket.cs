using UnityEngine;

public class Minimarket : MonoBehaviour
{
    public static Minimarket instance;

    [Header("Core References")]
    public Health health;
    public Transform restock_station;
    public Transform upgrade_station;

    [Header("Repair Configuration")]
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
        SetupStations();
    }

    private void SetupStations()
    {
        // 1. Freezer Station (Ammo Restock)
        if (restock_station == null)
        {
            restock_station = transform.Find("FreezerStation");
        }
        if (restock_station != null)
        {
            CircleCollider2D col = restock_station.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = restock_station.gameObject.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;
            col.radius = 1.3f;

            Interactable it = restock_station.GetComponent<Interactable>();
            if (it == null)
            {
                it = restock_station.gameObject.AddComponent<Interactable>();
            }
            it.SetActionLabel("RESTOCK AMMO");
            it.on_interacted.RemoveAllListeners();
            it.on_interacted.AddListener(OnRestockInteract);
        }

        // 2. Upgrade / Repair Station (Base Repair)
        if (upgrade_station == null)
        {
            upgrade_station = transform.Find("UpgradeStation");
        }
        if (upgrade_station != null)
        {
            CircleCollider2D col = upgrade_station.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = upgrade_station.gameObject.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;
            col.radius = 1.3f;

            Interactable it = upgrade_station.GetComponent<Interactable>();
            if (it == null)
            {
                it = upgrade_station.gameObject.AddComponent<Interactable>();
            }
            it.SetActionLabel("REPAIR BASE");
            it.on_interacted.RemoveAllListeners();
            it.on_interacted.AddListener(OnRepairInteract);
        }
    }

    public void OnRestockInteract(PlayerCombat combat)
    {
        if (combat == null && player_combat != null) combat = player_combat;
        if (combat == null) return;

        PlayerAmmo ammo = combat.GetComponent<PlayerAmmo>();
        if (ammo != null && ammo.ammo_carried < ammo.ammo_carried_max)
        {
            combat.Restock();
            if (LevelManager.instance != null) LevelManager.instance.RegisterRestock();
            NotificationToast.Show("Ammo restocked to MAX!", ToastType.Success);
        }
        else
        {
            NotificationToast.Show("Ammo is already full!", ToastType.Info);
        }
    }

    public void OnRepairInteract(PlayerCombat combat)
    {
        if (combat == null && player_combat != null) combat = player_combat;
        if (combat == null) return;

        if (health != null && health.health_current < health.health_max)
        {
            if (combat.repair_points >= repair_cost)
            {
                if (combat.UseRepairPoints(repair_cost))
                {
                    health.Heal(repair_heal_amount);
                    NotificationToast.Show("Minimarket repaired! +" + repair_heal_amount + " HP", ToastType.Success);
                }
            }
            else
            {
                NotificationToast.Show("Need " + repair_cost + " repair points to fix Minimarket! (" + combat.repair_points + "/" + repair_cost + ")", ToastType.Warning);
            }
        }
        else
        {
            NotificationToast.Show("Minimarket is already at full health!", ToastType.Info);
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
}
