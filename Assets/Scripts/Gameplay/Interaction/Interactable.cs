using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Interactable : MonoBehaviour
{
    [Header("Prompt Settings")]
    [SerializeField] private string action_label = "INTERACT";
    [SerializeField] private float interaction_radius = 2.5f;

    [Header("Visual Reference (Assign from hierarchy)")]
    [SerializeField] private TMP_Text prompt_text;

    [Header("Events")]
    public UnityEvent<PlayerCombat> on_interacted;

    private Transform player_transform;
    private PlayerCombat player_combat;
    private bool is_player_in_range = false;
    private Collider2D trigger_collider;

    private void Awake()
    {
        trigger_collider = GetComponent<Collider2D>();
        if (prompt_text == null)
        {
            prompt_text = GetComponentInChildren<TMP_Text>(true);
        }
        SetPromptVisible(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameConstants.Tags.Player))
        {
            ResolvePlayer(other.gameObject);
            is_player_in_range = true;
            UpdatePromptText();
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(GameConstants.Tags.Player))
        {
            is_player_in_range = false;
            SetPromptVisible(false);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            SetPromptVisible(false);
            return;
        }

        bool hasTrigger = trigger_collider != null && trigger_collider.isTrigger;
        if (!hasTrigger)
        {
            ResolvePlayer();
            if (player_transform == null)
            {
                SetPromptVisible(false);
                return;
            }

            float distance = trigger_collider != null
                ? Vector2.Distance(trigger_collider.ClosestPoint(player_transform.position), player_transform.position)
                : Vector2.Distance(player_transform.position, transform.position);

            is_player_in_range = distance <= interaction_radius;
        }

        if (is_player_in_range)
        {
            UpdatePromptText();
            SetPromptVisible(true);

            if (KeybindManager.WasPressedThisFrame(KeyAction.Interact))
            {
                TriggerInteract();
            }
        }
        else if (!hasTrigger)
        {
            SetPromptVisible(false);
        }
    }

    private void ResolvePlayer(GameObject source = null)
    {
        if (source != null)
        {
            player_combat = source.GetComponent<PlayerCombat>();
            if (player_combat == null) player_combat = source.GetComponentInParent<PlayerCombat>();
            if (player_combat != null)
            {
                player_transform = player_combat.transform;
                return;
            }
        }

        if (player_transform != null) return;

        if (PlayerCombat.Instance != null)
        {
            player_combat = PlayerCombat.Instance;
            player_transform = PlayerCombat.Instance.transform;
            return;
        }

        GameObject pGo = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
        if (pGo != null)
        {
            player_combat = pGo.GetComponent<PlayerCombat>();
            player_transform = pGo.transform;
        }
    }

    private void UpdatePromptText()
    {
        if (prompt_text == null) return;

        ActionBinding binding = KeybindManager.GetBinding(KeyAction.Interact);
        string keyName = binding.GetDisplayName();
        prompt_text.text = "[" + keyName + "] " + action_label.ToUpper();
    }

    private void SetPromptVisible(bool visible)
    {
        if (prompt_text != null)
        {
            prompt_text.gameObject.SetActive(visible);
        }
    }

    public void TriggerInteract()
    {
        if (on_interacted != null)
        {
            on_interacted.Invoke(player_combat);
        }
    }

    public void SetActionLabel(string label)
    {
        action_label = label;
        UpdatePromptText();
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
    }
}
