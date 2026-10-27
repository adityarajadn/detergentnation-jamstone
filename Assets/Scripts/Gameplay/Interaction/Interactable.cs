using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Interactable : MonoBehaviour
{
    [Header("Prompt Settings")]
    [SerializeField] private string action_label = "INTERACT";
    [SerializeField] private float interaction_radius = 2.5f;
    [SerializeField] private Vector3 prompt_offset = new Vector3(0f, 1.2f, 0f);

    [Header("Custom Visual (Optional)")]
    [SerializeField] private TextMeshPro custom_prompt_text;

    [Header("Events")]
    public UnityEvent<PlayerCombat> on_interacted;

    private TextMeshPro auto_prompt_text;
    private Transform player_transform;
    private PlayerCombat player_combat;
    private bool is_player_in_range = false;
    private Collider2D trigger_collider;

    private void Awake()
    {
        trigger_collider = GetComponent<Collider2D>();
        if (custom_prompt_text == null)
        {
            CreateAutoPrompt();
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

    private void CreateAutoPrompt()
    {
        Transform existing = transform.Find("AutoInteractionPrompt");
        if (existing != null)
        {
            auto_prompt_text = existing.GetComponent<TextMeshPro>();
            if (auto_prompt_text != null) return;
        }

        GameObject promptGo = new GameObject("AutoInteractionPrompt");
        promptGo.transform.SetParent(transform, false);
        promptGo.transform.localPosition = prompt_offset;

        auto_prompt_text = promptGo.AddComponent<TextMeshPro>();
        auto_prompt_text.fontSize = 4f;
        auto_prompt_text.alignment = TextAlignmentOptions.Center;
        auto_prompt_text.color = new Color(1f, 0.92f, 0.23f, 1f); // Accent yellow
        auto_prompt_text.sortingOrder = 200;

        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont != null) auto_prompt_text.font = defaultFont;
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

            Collider2D col = GetComponent<Collider2D>();
            float distance = col != null
                ? Vector2.Distance(col.ClosestPoint(player_transform.position), player_transform.position)
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
        ActionBinding binding = KeybindManager.GetBinding(KeyAction.Interact);
        string keyName = binding.GetDisplayName();
        string fullPrompt = "[" + keyName + "] " + action_label.ToUpper();

        if (custom_prompt_text != null)
        {
            custom_prompt_text.text = fullPrompt;
        }
        else if (auto_prompt_text != null)
        {
            auto_prompt_text.text = fullPrompt;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (custom_prompt_text != null)
        {
            custom_prompt_text.gameObject.SetActive(visible);
        }
        if (auto_prompt_text != null)
        {
            auto_prompt_text.gameObject.SetActive(visible);
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

    public void SetInteractionRadius(float radius)
    {
        interaction_radius = radius;
    }

    public void SetPromptOffset(Vector3 offset)
    {
        prompt_offset = offset;
        if (auto_prompt_text != null)
        {
            auto_prompt_text.transform.localPosition = prompt_offset;
        }
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
    }
}
