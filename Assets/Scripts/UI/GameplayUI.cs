using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gameplay HUD coordinator. Serialized widget references live here (scene references unchanged);
/// logic is delegated to the plain C# views in UI/Hud/: HudStatsView, HudWaveView,
/// HudDefenseView, HudEndScreens, HudPauseMenu.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [Header("Player & Combat")]
    [SerializeField] private Slider player_health_slider;
    [SerializeField] private TMP_Text ammo_text;
    [SerializeField] private TMP_Text candy_text;
    [SerializeField] private TMP_Text repair_points_text;

    [Header("Consumables HUD")]
    [SerializeField] private GameObject consumable_panel;
    [SerializeField] private TMP_Text consumable_slot1_text;
    [SerializeField] private TMP_Text consumable_slot2_text;
    [SerializeField] private Image consumable_slot1_icon;
    [SerializeField] private Image consumable_slot2_icon;
    [SerializeField] private Sprite icon_soap_bomb;
    [SerializeField] private Sprite icon_first_aid;
    [SerializeField] private Sprite icon_tools_box;
    [SerializeField] private Sprite icon_energy_drink;
    [SerializeField] private Sprite icon_soap_bucket;
    [SerializeField] private Sprite icon_sonar_batarang;

    [Header("Minimarket")]
    [SerializeField] private Slider minimarket_health_slider;

    [Header("Wave Progress")]
    [SerializeField] private TMP_Text wave_text;
    [SerializeField] private GameObject huge_wave_banner;
    [SerializeField] private TMP_Text break_timer_text;
    [SerializeField] private Slider wave_progress_slider;

    [Header("Planning Phase")]
    [SerializeField] private GameObject planning_panel;
    [SerializeField] private TMP_Text defense_points_text;
    [SerializeField] private Button start_wave_button;
    [SerializeField] private Button defense_btn_fence;
    [SerializeField] private Button defense_btn_cannon;
    [SerializeField] private Button defense_btn_trap;
    [SerializeField] private Button defense_btn_barrel;
    [SerializeField] private Button defense_btn_refund;

    [Header("Kill Streak")]
    [SerializeField] private GameObject streak_panel;
    [SerializeField] private TMP_Text streak_text;

    [Header("Pickup Feedback UI")]
    [SerializeField] private CanvasGroup pickup_feedback_group;
    [SerializeField] private TMP_Text pickup_feedback_text;

    [Header("Game Over / Victory")]
    [SerializeField] private GameObject victory_panel;
    [SerializeField] private TMP_Text victory_stars_text;
    [SerializeField] private Button victory_next_button;
    [SerializeField] private Button victory_retry_button;
    [SerializeField] private Button victory_menu_button;

    [Header("Defeat")]
    [SerializeField] private GameObject defeat_panel;
    [SerializeField] private Button defeat_retry_button;
    [SerializeField] private Button defeat_menu_button;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pause_panel;
    [SerializeField] private Button pause_resume_button;
    [SerializeField] private Button pause_settings_button;
    [SerializeField] private Button pause_menu_button;
    [SerializeField] private SettingsUI settings_panel;

    public static GameplayUI Instance { get; private set; }

    private HudStatsView stats_view;
    private HudWaveView wave_view;
    private HudDefenseView defense_view;
    private HudEndScreens end_screens;
    private HudPauseMenu pause_menu;

    private Coroutine pickup_feedback_routine;

    private void Awake()
    {
        Instance = this;
        BuildViews();
    }

    private void BuildViews()
    {
        stats_view = new HudStatsView
        {
            player_health_slider = player_health_slider,
            ammo_text = ammo_text,
            candy_text = candy_text,
            repair_points_text = repair_points_text,
            consumable_panel = consumable_panel,
            consumable_slot1_text = consumable_slot1_text,
            consumable_slot2_text = consumable_slot2_text,
            consumable_slot1_icon = consumable_slot1_icon,
            consumable_slot2_icon = consumable_slot2_icon,
            icon_soap_bomb = icon_soap_bomb,
            icon_first_aid = icon_first_aid,
            icon_tools_box = icon_tools_box,
            icon_energy_drink = icon_energy_drink,
            icon_soap_bucket = icon_soap_bucket,
            icon_sonar_batarang = icon_sonar_batarang,
            minimarket_health_slider = minimarket_health_slider
        };

        wave_view = new HudWaveView(this)
        {
            wave_text = wave_text,
            huge_wave_banner = huge_wave_banner,
            break_timer_text = break_timer_text,
            wave_progress_slider = wave_progress_slider
        };

        defense_view = new HudDefenseView
        {
            planning_panel = planning_panel,
            defense_points_text = defense_points_text,
            start_wave_button = start_wave_button,
            defense_btn_fence = defense_btn_fence,
            defense_btn_cannon = defense_btn_cannon,
            defense_btn_trap = defense_btn_trap,
            defense_btn_barrel = defense_btn_barrel,
            defense_btn_refund = defense_btn_refund,
        };

        end_screens = new HudEndScreens
        {
            victory_panel = victory_panel,
            victory_stars_text = victory_stars_text,
            victory_next_button = victory_next_button,
            victory_retry_button = victory_retry_button,
            victory_menu_button = victory_menu_button,
            defeat_panel = defeat_panel,
            defeat_retry_button = defeat_retry_button,
            defeat_menu_button = defeat_menu_button
        };

        pause_menu = new HudPauseMenu
        {
            pause_panel = pause_panel,
            pause_resume_button = pause_resume_button,
            pause_settings_button = pause_settings_button,
            pause_menu_button = pause_menu_button,
            settings_panel = settings_panel
        };
    }

    private void Start()
    {
        stats_view.FindPlayerReferences();

        defense_view.Bind();
        end_screens.Bind();
        wave_view.Bind();
        pause_menu.Bind();

        if (KillStreakManager.instance != null)
        {
            KillStreakManager.instance.on_streak_changed.AddListener(OnStreakChanged);
            KillStreakManager.instance.on_streak_reset.AddListener(OnStreakReset);
        }

        if (streak_panel != null) streak_panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (pause_menu != null) pause_menu.Unbind();
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        stats_view.Tick();
    }

    public Sprite GetConsumableSprite(string itemName)
    {
        return stats_view.GetConsumableSprite(itemName);
    }

    private void OnStreakChanged(int count, int bonus)
    {
        if (streak_panel != null && count >= 2)
        {
            streak_panel.SetActive(true);
            if (streak_text != null)
            {
                streak_text.text = count + "x Streak!" + (bonus > 0 ? " (+" + bonus + " Candy)" : "");
            }
        }
    }

    private void OnStreakReset()
    {
        if (streak_panel != null) streak_panel.SetActive(false);
    }

    public void ShowPickupFeedback(string message)
    {
        if (pickup_feedback_text == null || pickup_feedback_group == null) return;

        pickup_feedback_text.text = message;

        if (pickup_feedback_routine != null)
        {
            StopCoroutine(pickup_feedback_routine);
        }
        pickup_feedback_routine = StartCoroutine(PickupFeedbackRoutine());
    }

    private System.Collections.IEnumerator PickupFeedbackRoutine()
    {
        pickup_feedback_group.gameObject.SetActive(true);
        float fadeIn = 0.15f;
        float hold = 0.85f;
        float fadeOut = 0.35f;

        // Fade in
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            pickup_feedback_group.alpha = Mathf.Clamp01(t / fadeIn);
            yield return null;
        }
        pickup_feedback_group.alpha = 1f;

        // Hold
        yield return new WaitForSecondsRealtime(hold);

        // Fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            pickup_feedback_group.alpha = Mathf.Clamp01(1f - (t / fadeOut));
            yield return null;
        }

        pickup_feedback_group.alpha = 0f;
        pickup_feedback_group.gameObject.SetActive(false);
        pickup_feedback_routine = null;
    }

    public void TogglePause()
    {
        pause_menu.Toggle();
    }
}
