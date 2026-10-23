using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInteraction : MonoBehaviour
{
    public Player player;
    public int repairPoints = 0;
    public int activeSlot = 0;
    public GameObject soapBombPrefab;

    public void AddRepairPoints(int pts) => repairPoints += pts;

    private void Update()
    {
        bool interactPress = Input.GetKeyDown(KeybindManager.Interact) || Input.GetKeyDown(KeyCode.E);
        bool useItemPress = Input.GetKeyDown(KeybindManager.UseItem) || Input.GetKeyDown(KeyCode.Q);
        bool slot1Press = Input.GetKeyDown(KeyCode.Alpha1);
        bool slot2Press = Input.GetKeyDown(KeyCode.Alpha2);

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.eKey.wasPressedThisFrame) interactPress = true;
            if (kb.qKey.wasPressedThisFrame) useItemPress = true;
            if (kb.digit1Key.wasPressedThisFrame) slot1Press = true;
            if (kb.digit2Key.wasPressedThisFrame) slot2Press = true;
        }
#endif

        // Interaksi dengan Toko Minimarket
        if (interactPress)
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var col in overlaps)
            {
                if (col.TryGetComponent<Minimarket>(out var store))
                {
                    if (store.playerInRestockZone)
                    {
                        if (player != null && player.combat != null) player.combat.carriedAmmo = 64;
                        AudioManager.Instance?.PlaySfx("reload");
                        return;
                    }
                    else if (store.playerInRepairZone && repairPoints > 0 && store.healthCurrent < store.healthMax)
                    {
                        int needed = store.healthMax - store.healthCurrent;
                        int use = Mathf.Min(needed, repairPoints);
                        repairPoints -= use;
                        store.Repair(use);
                        return;
                    }
                }
            }
        }

        // Slot Switch
        if (slot1Press) activeSlot = 0;
        if (slot2Press) activeSlot = 1;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
#if ENABLE_INPUT_SYSTEM
        var ms = Mouse.current;
        if (ms != null)
        {
            float sVal = ms.scroll.ReadValue().y;
            if (sVal > 0f) activeSlot = 0;
            if (sVal < 0f) activeSlot = 1;
        }
#endif
        if (scroll > 0f) activeSlot = 0;
        if (scroll < 0f) activeSlot = 1;

        // Use consumable
        if (useItemPress && player != null && !player.isPlanningMode)
        {
            UseActiveConsumable();
        }
    }

    private void UseActiveConsumable()
    {
        if (GameState.Instance == null) return;
        var list = GameState.Instance.data.equippedConsumables;
        if (activeSlot >= list.Count) return;
        string id = list[activeSlot];
        int qty = GameState.Instance.GetConsumableQty(id);
        if (qty <= 0) return;

        GameState.Instance.ModifyConsumableQty(id, -1);

        switch (id)
        {
            case "soap_bomb":
                if (soapBombPrefab != null && Camera.main != null && player != null)
                {
                    Vector3 muz = player.muzzle != null ? player.muzzle.position : transform.position;
                    Vector2 throwDir = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)muz).normalized;
                    GameObject bomb = Instantiate(soapBombPrefab, muz, Quaternion.identity);
                    var rb = bomb.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = throwDir * 9.0f + new Vector2(0, 3.5f);
                }
                break;
            case "first_aid":
                player?.health?.Heal(50);
                break;
            case "energy_drink":
                if (player?.movement != null)
                {
                    player.movement.speedMultiplier = 1.45f;
                    player.movement.speedBoostTimer = 8.0f;
                }
                AudioManager.Instance?.PlaySfx("streak");
                break;
            case "soap_bucket":
                if (player?.health != null) player.health.shieldCharges += 3;
                AudioManager.Instance?.PlaySfx("repair");
                break;
            case "tools_box":
                AddRepairPoints(30);
                break;
            case "sonar_batarang":
                AudioManager.Instance?.PlaySfx("alarm");
                foreach (var enemy in FindObjectsByType<EnemyBase>(FindObjectsInactive.Exclude))
                {
                    enemy.Stun(10.0f);
                }
                break;
        }
    }
}
