using UnityEngine;

public class DefensePlacementSlot : MonoBehaviour
{
    public Transform spawn_point;
    public GameObject current_defense_instance;
    public DefenseData current_defense_data;

    [Header("Indicator Colors")]
    [SerializeField] private Color hover_color = new Color(0.3f, 1f, 0.4f, 0.9f);
    [SerializeField] private Color idle_color = new Color(0.2f, 0.7f, 1f, 0.5f);

    private SpriteRenderer indicator;
    private Collider2D slot_collider;

    private void Awake()
    {
        indicator = GetComponent<SpriteRenderer>();
        slot_collider = GetComponent<Collider2D>();
        if (spawn_point == null) spawn_point = transform;
    }

    private void Start()
    {
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
            if (should_show) indicator.color = idle_color;
        }

        if (slot_collider != null)
        {
            slot_collider.enabled = should_show;
        }
    }

    private void Update()
    {
        UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            GameObject player = PlayerController.Instance != null ? PlayerController.Instance.gameObject : GameObject.FindWithTag(GameConstants.Tags.Player);
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
            indicator.color = hover_color;
        }
    }

    private void OnMouseExit()
    {
        if (indicator != null && !HasDefense())
        {
            indicator.color = idle_color;
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
            GameObject obj = DefensePlacementManager.instance.PlaceDefense(data, spawn_point.position, null);
            if (obj != null)
            {
                obj.transform.localScale = data.prefab.transform.localScale;
                current_defense_instance = obj;
                current_defense_data = data;

                DefenseObject def_obj = obj.GetComponent<DefenseObject>();
                if (def_obj != null)
                {
                    def_obj.on_destroyed += OnPlacedDefenseDestroyed;
                }

                if (indicator != null) indicator.enabled = false;
                if (slot_collider != null) slot_collider.enabled = false;
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
