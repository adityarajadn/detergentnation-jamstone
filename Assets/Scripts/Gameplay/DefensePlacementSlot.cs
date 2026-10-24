using UnityEngine;

public class DefensePlacementSlot : MonoBehaviour
{
    public Transform spawn_point;
    public GameObject current_defense_instance;
    public DefenseData current_defense_data;

    private SpriteRenderer indicator;

    private void Awake()
    {
        indicator = GetComponent<SpriteRenderer>();
        if (spawn_point == null) spawn_point = transform;
    }

    private void Update()
    {
        UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && Vector2.Distance(player.transform.position, transform.position) <= 2.2f)
            {
                HandleInteraction();
            }
        }
    }

    private void OnMouseDown()
    {
        HandleInteraction();
    }

    private void OnMouseEnter()
    {
        if (indicator != null && !HasDefense())
        {
            indicator.color = new Color(0.3f, 1f, 0.4f, 0.9f);
        }
    }

    private void OnMouseExit()
    {
        if (indicator != null && !HasDefense())
        {
            indicator.color = new Color(0.2f, 0.7f, 1f, 0.5f);
        }
    }

    public void HandleInteraction()
    {
        if (LevelManager.instance != null && LevelManager.instance.current_phase != GamePhase.Planning) return;
        if (DefensePlacementManager.instance == null) return;

        if (DefensePlacementManager.instance.is_refund_mode)
        {
            if (HasDefense()) RemoveDefense();
            return;
        }

        if (DefensePlacementManager.instance.selected_defense != null)
        {
            if (!HasDefense())
            {
                TryPlace(DefensePlacementManager.instance.selected_defense);
            }
            return;
        }

        if (HasDefense())
        {
            RemoveDefense();
        }
    }

    public bool HasDefense()
    {
        return current_defense_instance != null;
    }

    public bool TryPlace(DefenseData data)
    {
        if (HasDefense() || data == null) return false;

        if (DefensePlacementManager.instance != null && DefensePlacementManager.instance.CanAfford(data))
        {
            GameObject obj = DefensePlacementManager.instance.PlaceDefense(data, spawn_point.position, transform);
            if (obj != null)
            {
                current_defense_instance = obj;
                current_defense_data = data;
                if (indicator != null) indicator.enabled = false;
                return true;
            }
        }

        return false;
    }

    public void RemoveDefense()
    {
        if (!HasDefense()) return;

        if (DefensePlacementManager.instance != null && current_defense_data != null)
        {
            DefensePlacementManager.instance.RefundDefense(current_defense_data);
        }

        Destroy(current_defense_instance);
        current_defense_instance = null;
        current_defense_data = null;
        if (indicator != null)
        {
            indicator.enabled = true;
            indicator.color = new Color(0.2f, 0.7f, 1f, 0.5f);
        }
    }
}
