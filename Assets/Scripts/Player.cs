using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Components")]
    public PlayerMovement movement;
    public PlayerHealth health;
    public PlayerCombat combat;
    public PlayerInteraction interaction;

    [Header("Mount Points")]
    public Transform visualBody;
    public Transform aimArm;
    public Transform muzzle;
    public Transform meleeMount;
    public Transform armorMount;
    public Transform jetpackMount;

    [Header("Camera & Visuals")]
    public Camera mainCamera;
    public bool isPlanningMode = false;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        ApplyEquipmentStats();
    }

    public void ApplyEquipmentStats()
    {
        if (GameState.Instance == null) return;
        if (movement != null) movement.SetupEquipment(GameState.Instance.data.equippedJetpack);
        if (health != null) health.SetupArmor(GameState.Instance.data.equippedArmor);
        if (combat != null) combat.SetupWeapons(GameState.Instance.data.equippedRange, GameState.Instance.data.equippedMelee);
    }

    public void TakeDamage(int dmg) => health?.TakeDamage(dmg);
    public void Heal(int pts) => health?.Heal(pts);
    public void AddRepairPoints(int pts) => interaction?.AddRepairPoints(pts);
}
