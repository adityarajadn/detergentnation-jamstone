using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum FacingDirection
{
    Right,
    Left
}

public class DefensePlacementSlot : MonoBehaviour, IInteractable
{
    [Header("Placement Orientation")]
    [SerializeField] private FacingDirection slot_direction = FacingDirection.Right;
    public FacingDirection SlotDirection => slot_direction;

    public Transform spawn_point;
    public GameObject current_defense_instance;
    public DefenseData current_defense_data;

    public string ActionLabel => HasDefense() ? "REMOVE DEFENSE" : "PLACE DEFENSE";

    public void OnInteract(PlayerCombat player)
    {
        HandleInteraction();
    }

    private SpriteRenderer indicator;
    private Collider2D slot_collider;

    private readonly Color normal_color = new Color(0.2f, 0.7f, 1f, 0.5f);
    private readonly Color hover_color = new Color(1f, 0.95f, 0.3f, 0.85f);
    private bool is_hovered = false;

    private void Awake()
    {
        indicator = GetComponent<SpriteRenderer>();
        slot_collider = GetComponent<Collider2D>();
        if (spawn_point == null) spawn_point = transform;
    }

    private void Start()
    {
        if (indicator != null)
        {
            indicator.color = normal_color;
        }

        if (LevelManager.instance != null)
        {
            LevelManager.instance.on_phase_changed.AddListener(OnPhaseChanged);
            UpdateSlotVisibility(LevelManager.instance.current_phase);
        }
        else
        {
            UpdateSlotVisibility(GamePhase.Planning);
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.on_phase_changed.RemoveListener(OnPhaseChanged);
        }
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        UpdateSlotVisibility(phase);
    }

    private void UpdateSlotVisibility(GamePhase phase)
    {
        bool is_planning = phase == GamePhase.Planning;
        bool should_show = is_planning && !HasDefense();

        if (indicator != null)
        {
            indicator.enabled = should_show;
            indicator.color = normal_color;
        }

        if (slot_collider != null)
        {
            slot_collider.enabled = should_show;
        }

        is_hovered = false;
    }

    private void Update()
    {
        if (LevelManager.instance != null && LevelManager.instance.current_phase != GamePhase.Planning)
        {
            return;
        }

        HandleMouseDetection();
        HandleKeyboardInteraction();
    }

    private void HandleKeyboardInteraction()
    {
        if (KeybindManager.WasPressedThisFrame(KeyAction.Interact))
        {
            PlayerController pc = PlayerController.Instance;
            if (pc != null && Vector2.Distance(pc.transform.position, transform.position) <= 2.5f)
            {
                HandleInteraction();
            }
        }
    }

    private void HandleMouseDetection()
    {
        if (slot_collider == null || !slot_collider.enabled) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 mouse_pos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        Vector2 world_pos = cam.ScreenToWorldPoint(mouse_pos);

        bool pointer_over_slot = slot_collider.OverlapPoint(world_pos);

        // Check if pointer is blocked by UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            pointer_over_slot = false;
        }

        if (pointer_over_slot != is_hovered)
        {
            is_hovered = pointer_over_slot;
            if (indicator != null)
            {
                indicator.color = is_hovered ? hover_color : normal_color;
            }
        }

        bool mouse_clicked = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || Input.GetMouseButtonDown(0);

        if (is_hovered && mouse_clicked)
        {
            HandleInteraction();
        }
    }

    private void OnMouseEnter()
    {
        if (slot_collider != null && slot_collider.enabled)
        {
            is_hovered = true;
            if (indicator != null) indicator.color = hover_color;
        }
    }

    private void OnMouseExit()
    {
        is_hovered = false;
        if (indicator != null) indicator.color = normal_color;
    }

    private void OnMouseDown()
    {
        // Fallback for Unity's built-in click
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        HandleInteraction();
    }

    public void HandleInteraction()
    {
        if (LevelManager.instance != null && LevelManager.instance.current_phase != GamePhase.Planning)
        {
            NotificationToast.Show("Can only place defenses during Planning Phase!", ToastType.Warning);
            return;
        }
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
                if (!DefensePlacementManager.instance.CanAfford(DefensePlacementManager.instance.selected_defense))
                {
                    NotificationToast.Show("Not enough points to place " + DefensePlacementManager.instance.selected_defense.defense_name + "!", ToastType.Warning);
                    return;
                }
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
            GameObject obj = DefensePlacementManager.instance.PlaceDefense(data, spawn_point.position, null);
            if (obj != null)
            {
                obj.transform.localScale = data.prefab.transform.localScale;
                current_defense_instance = obj;
                current_defense_data = data;

                DefenseObject def_obj = obj.GetComponent<DefenseObject>();
                if (def_obj != null)
                {
                    def_obj.SetFacingDirection(slot_direction);
                    def_obj.on_destroyed += OnPlacedDefenseDestroyed;
                }

                if (indicator != null) indicator.enabled = false;
                if (slot_collider != null) slot_collider.enabled = false;
                NotificationToast.Show(data.defense_name + " placed!", ToastType.Success);
                return true;
            }
        }

        return false;
    }

    private void OnPlacedDefenseDestroyed()
    {
        current_defense_instance = null;
        current_defense_data = null;
        GamePhase phase = LevelManager.instance != null ? LevelManager.instance.current_phase : GamePhase.Planning;
        UpdateSlotVisibility(phase);
    }

    public void RemoveDefense()
    {
        if (!HasDefense()) return;

        if (DefensePlacementManager.instance != null && current_defense_data != null)
        {
            DefensePlacementManager.instance.RefundDefense(current_defense_data);
        }

        if (current_defense_instance != null)
        {
            DefenseObject def_obj = current_defense_instance.GetComponent<DefenseObject>();
            if (def_obj != null)
            {
                def_obj.on_destroyed -= OnPlacedDefenseDestroyed;
            }
            Destroy(current_defense_instance);
        }

        current_defense_instance = null;
        current_defense_data = null;
        GamePhase phase = LevelManager.instance != null ? LevelManager.instance.current_phase : GamePhase.Planning;
        UpdateSlotVisibility(phase);
    }
}
