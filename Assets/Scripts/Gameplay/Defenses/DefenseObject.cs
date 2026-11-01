using System.Collections;
using TMPro;
using UnityEngine;

public class DefenseObject : MonoBehaviour
{
    [Header("Base Defense Info")]
    public DefenseData defense_data;
    public Health health;
    public int ammo_current;

    [SerializeField] protected TMP_Text prompt_text;
    [SerializeField] protected Transform visuals;

    protected GameObject visuals_container;
    public System.Action on_destroyed;

    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        if (visuals != null) visuals_container = visuals.gameObject;
        else
        {
            Transform vis = transform.Find("Visuals");
            if (vis != null) visuals_container = vis.gameObject;
        }
        if (prompt_text == null) prompt_text = GetComponentInChildren<TMP_Text>();
    }

    protected virtual void Start()
    {
        if (defense_data != null)
        {
            if (health != null && defense_data.max_health > 0)
            {
                health.SetMaxHealth(defense_data.max_health, true);
            }
            ammo_current = defense_data.max_ammo;
        }

        if (health != null)
        {
            health.on_death.AddListener(OnDestroyed);
            health.on_health_changed.AddListener(OnHealthChanged);
        }

        HidePrompt();
    }

    protected virtual void Update()
    {
        HandlePlayerProximityInteraction();
    }

    protected virtual void OnHealthChanged(int cur, int max) { }

    protected virtual void HandlePlayerProximityInteraction()
    {
        if (LevelManager.instance != null && LevelManager.instance.current_phase == GamePhase.Planning)
        {
            HidePrompt();
            return;
        }

        GameObject player = PlayerController.Instance != null ? PlayerController.Instance.gameObject : GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
        if (player == null)
        {
            HidePrompt();
            return;
        }

        float dist = Vector2.Distance(player.transform.position, transform.position);
        if (dist <= GetInteractionDistance() && CanInteract())
        {
            ShowPrompt(GetInteractionPrompt(GetInteractKeyName()));
            if (KeybindManager.WasPressedThisFrame(KeyAction.Interact) || (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)))
            {
                OnInteract();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    protected virtual float GetInteractionDistance() => 2.2f;
    protected virtual bool CanInteract() => false;
    protected virtual string GetInteractionPrompt(string keyName) => "";
    protected virtual void OnInteract() { }

    protected string GetInteractKeyName()
    {
        ActionBinding binding = KeybindManager.GetBinding(KeyAction.Interact);
        if (binding.device == InputDeviceType.Keyboard)
        {
            return binding.key.ToString();
        }
        return "F";
    }

    protected void ShowPrompt(string message)
    {
        if (prompt_text != null)
        {
            prompt_text.gameObject.SetActive(true);
            prompt_text.text = message;
        }
    }

    protected void HidePrompt()
    {
        if (prompt_text != null) prompt_text.gameObject.SetActive(false);
    }

    public virtual void OnDestroyed()
    {
        PlayBreakAudio();
        on_destroyed?.Invoke();
        on_destroyed = null;
        Destroy(gameObject);
    }

    protected virtual void PlayBreakAudio() { }

    protected virtual void OnDestroy()
    {
        on_destroyed?.Invoke();
        on_destroyed = null;
    }
}
