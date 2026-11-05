using DG.Tweening;
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
    [SerializeField] private UnityEngine.UI.Image streak_background_img;
    [SerializeField] private Sprite streak_bg_normal;
    [SerializeField] private Sprite streak_bg_high;
    private Tween streak_tween;

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

    private const float PickupFadeInSeconds = 0.15f;
    private const float PickupHoldSeconds = 0.85f;
    private const float PickupFadeOutSeconds = 0.35f;

    private Sequence pickup_feedback_tween;

    private void Awake()
    {
        Instance = this;
        ServiceLocator.Register<GameplayUI>(this);
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

    private void OnDisable()
    {
        KillPickupFeedback();
        streak_tween?.Kill();
        streak_tween = null;
    }

    private void OnDestroy()
    {
        KillPickupFeedback();
        streak_tween?.Kill();
        streak_tween = null;
        if (pause_menu != null) pause_menu.Unbind();
        ServiceLocator.Unregister<GameplayUI>();
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
        if (streak_panel == null || count < 2) return;

        bool wasActive = streak_panel.activeSelf;
        streak_panel.SetActive(true);

        if (streak_background_img != null)
        {
            streak_background_img.sprite = count >= 10 && streak_bg_high != null ? streak_bg_high : streak_bg_normal;
        }

        if (streak_text != null)
        {
            streak_text.text = count + "x Streak!" + (bonus > 0 ? " (+" + bonus + " Candy)" : "");
        }

        streak_tween?.Kill();
        if (!wasActive)
        {
            streak_panel.transform.localScale = Vector3.zero;
            streak_tween = streak_panel.transform.DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
        else
        {
            streak_panel.transform.localScale = Vector3.one;
            float punchAmount = count >= 10 ? 0.35f : 0.2f;
            streak_tween = streak_panel.transform.DOPunchScale(new Vector3(punchAmount, punchAmount, 0f), 0.2f, 10, 1)
                .SetUpdate(true);
        }
    }

    private void OnStreakReset()
    {
        if (streak_panel == null || !streak_panel.activeSelf) return;

        streak_tween?.Kill();
        streak_tween = streak_panel.transform.DOScale(0f, 0.18f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (streak_panel != null) streak_panel.SetActive(false);
                streak_tween = null;
            });
    }

    public void ShowPickupFeedback(string message)
    {
        if (pickup_feedback_text == null || pickup_feedback_group == null) return;

        pickup_feedback_text.text = message;

        KillPickupFeedback();

        pickup_feedback_group.gameObject.SetActive(true);
        pickup_feedback_group.alpha = 0f;

        pickup_feedback_tween = DOTween.Sequence().SetUpdate(true);
        pickup_feedback_tween.Append(pickup_feedback_group.DOFade(1f, PickupFadeInSeconds));
        pickup_feedback_tween.AppendInterval(PickupHoldSeconds);
        pickup_feedback_tween.Append(pickup_feedback_group.DOFade(0f, PickupFadeOutSeconds));
        pickup_feedback_tween.OnComplete(() =>
        {
            pickup_feedback_group.gameObject.SetActive(false);
            pickup_feedback_tween = null;
        });
    }

    private void KillPickupFeedback()
    {
        if (pickup_feedback_tween != null)
        {
            pickup_feedback_tween.Kill();
            pickup_feedback_tween = null;
        }
    }

    public void TogglePause()
    {
        pause_menu.Toggle();
    }
}
