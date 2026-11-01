using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Image consumable_slot1_bg;
    [SerializeField] private Image consumable_slot2_bg;
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

    [Header("UI Theme Colors")]
    [SerializeField] private Color slot_active_color = new Color(0.2f, 0.7f, 0.9f, 0.9f);
    [SerializeField] private Color slot_inactive_color = new Color(0.12f, 0.14f, 0.2f, 0.75f);
    [SerializeField] private Color defense_btn_selected_color = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color defense_btn_normal_color = new Color(0.18f, 0.22f, 0.32f, 0.95f);

    public static GameplayUI Instance { get; private set; }

    private Coroutine pickup_feedback_routine;

    private void Awake()
    {
        Instance = this;
    }

    private PlayerCombat player_combat;
    private PlayerAmmo player_ammo;
    private PlayerConsumableInventory player_consumable_inv;
    private Health player_health;

    private void Start()
    {
        FindPlayerReferences();

        if (LevelManager.instance != null)
        {
            LevelManager.instance.on_phase_changed.AddListener(OnPhaseChanged);
            LevelManager.instance.on_victory.AddListener(OnVictory);
            LevelManager.instance.on_defeat.AddListener(OnDefeat);
        }

        if (WaveManager.instance != null)
        {
            WaveManager.instance.on_wave_started.AddListener(OnWaveStarted);
            WaveManager.instance.on_huge_wave_alert.AddListener(OnHugeWaveAlert);
            WaveManager.instance.on_break_started.AddListener(OnBreakStarted);
            WaveManager.instance.on_break_timer_tick.AddListener(OnBreakTimerTick);
            WaveManager.instance.on_break_ended.AddListener(OnBreakEnded);
        }

        if (DefensePlacementManager.instance != null)
        {
            DefensePlacementManager.instance.on_points_changed.AddListener(OnDefensePointsChanged);
            DefensePlacementManager.instance.on_selection_changed.AddListener(OnDefenseSelectionChanged);

            var defs = DefensePlacementManager.instance.available_defenses;
            if (defense_btn_fence != null && defs.Count > 0)
                defense_btn_fence.onClick.AddListener(() => DefensePlacementManager.instance.SelectDefense(defs[0]));
            if (defense_btn_cannon != null && defs.Count > 1)
                defense_btn_cannon.onClick.AddListener(() => DefensePlacementManager.instance.SelectDefense(defs[1]));
            if (defense_btn_trap != null && defs.Count > 2)
                defense_btn_trap.onClick.AddListener(() => DefensePlacementManager.instance.SelectDefense(defs[2]));
            if (defense_btn_barrel != null && defs.Count > 3)
                defense_btn_barrel.onClick.AddListener(() => DefensePlacementManager.instance.SelectDefense(defs[3]));
            if (defense_btn_refund != null)
                defense_btn_refund.onClick.AddListener(() => DefensePlacementManager.instance.ToggleRefundMode());

            UpdateDefenseButtonStates(DefensePlacementManager.instance.defense_points_current);
        }

        if (KillStreakManager.instance != null)
        {
            KillStreakManager.instance.on_streak_changed.AddListener(OnStreakChanged);
            KillStreakManager.instance.on_streak_reset.AddListener(OnStreakReset);
        }

        if (start_wave_button != null)
        {
            start_wave_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.StartCombatPhase();
            });
        }

        if (victory_next_button != null)
        {
            victory_next_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.NextLevel();
            });
        }

        if (victory_retry_button != null)
        {
            victory_retry_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.RestartCurrentLevel();
            });
        }

        if (victory_menu_button != null)
        {
            victory_menu_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.ReturnToMainMenu();
            });
        }

        if (defeat_retry_button != null)
        {
            defeat_retry_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.RestartCurrentLevel();
            });
        }

        if (defeat_menu_button != null)
        {
            defeat_menu_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.ReturnToMainMenu();
            });
        }

        if (pause_resume_button != null)
        {
            pause_resume_button.onClick.AddListener(ResumeGame);
        }
        if (pause_settings_button != null)
        {
            pause_settings_button.onClick.AddListener(() =>
            {
                if (settings_panel != null)
                {
                    if (pause_panel != null) pause_panel.SetActive(false);
                    settings_panel.gameObject.SetActive(true);
                }
                else
                {
                    Time.timeScale = 1f;
                    SceneLoader.LoadSettings();
                }
            });
        }
        if (settings_panel != null)
        {
            settings_panel.on_closed += () =>
            {
                if (pause_panel != null && Time.timeScale == 0f)
                {
                    pause_panel.SetActive(true);
                }
            };
            settings_panel.gameObject.SetActive(false);
        }
        if (pause_menu_button != null)
        {
            pause_menu_button.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneLoader.LoadMainMenu();
            });
        }
        if (victory_panel != null) victory_panel.SetActive(false);
        if (defeat_panel != null) defeat_panel.SetActive(false);
        if (huge_wave_banner != null) huge_wave_banner.SetActive(false);
        if (streak_panel != null) streak_panel.SetActive(false);
        if (pause_panel != null) pause_panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        UpdatePlayerUI();
        UpdateMinimarketUI();
    }

    private void FindPlayerReferences()
    {
        if (PlayerCombat.Instance != null)
        {
            player_combat = PlayerCombat.Instance;
            player_ammo = PlayerCombat.Instance.GetComponent<PlayerAmmo>();
            player_consumable_inv = PlayerCombat.Instance.GetComponent<PlayerConsumableInventory>();
            player_health = PlayerCombat.Instance.GetComponent<Health>();
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
        if (player != null)
        {
            player_combat = player.GetComponent<PlayerCombat>();
            player_ammo = player.GetComponent<PlayerAmmo>();
            player_consumable_inv = player.GetComponent<PlayerConsumableInventory>();
            player_health = player.GetComponent<Health>();
        }
    }

    private void UpdatePlayerUI()
    {
        if (player_health == null || player_combat == null)
        {
            FindPlayerReferences();
            return;
        }

        if (player_health_slider != null)
        {
            player_health_slider.maxValue = player_health.health_max;
            player_health_slider.value = player_health.health_current;
        }

        if (ammo_text != null)
        {
            if (player_combat.active_weapon != null)
            {
                ammo_text.text = player_combat.active_weapon.GetAmmoDisplayText(player_ammo);
            }
            else if (player_combat.active_weapon_type == WeaponType.Melee)
            {
                ammo_text.text = player_combat.melee_weapon != null ? player_combat.melee_weapon.weapon_name : "MELEE";
            }
            else
            {
                int inMag = player_ammo != null ? player_ammo.ammo_in_mag : 0;
                int carried = player_ammo != null ? player_ammo.ammo_carried : 0;
                ammo_text.text = inMag + " / " + carried;
            }
        }

        if (candy_text != null && GameManager.instance != null)
        {
            candy_text.text = GameManager.instance.candy_total.ToString();
        }

        if (repair_points_text != null)
        {
            repair_points_text.text = player_combat.repair_points.ToString();
        }

        UpdateConsumableUI();
    }

    private void UpdateConsumableUI()
    {
        if (consumable_panel == null) return;

        string c1_name = GameManager.instance != null ? GameManager.instance.equipped_consumable_1 : GameConstants.EquipmentNames.DefaultConsumable1;
        string c2_name = GameManager.instance != null ? GameManager.instance.equipped_consumable_2 : GameConstants.EquipmentNames.DefaultConsumable2;

        int c1_count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(c1_name) : 0;
        int c2_count = GameManager.instance != null ? GameManager.instance.GetConsumableCount(c2_name) : 0;

        int active_slot = player_consumable_inv != null ? player_consumable_inv.active_consumable_slot : 1;

        if (consumable_slot1_text != null)
        {
            consumable_slot1_text.text = c1_count.ToString();
        }

        if (consumable_slot2_text != null)
        {
            consumable_slot2_text.text = c2_count.ToString();
        }

        if (consumable_slot1_icon != null)
        {
            Sprite s1 = GetConsumableSprite(c1_name);
            consumable_slot1_icon.sprite = s1;
            consumable_slot1_icon.color = s1 != null ? Color.white : Color.clear;
            consumable_slot1_icon.enabled = s1 != null;
        }

        if (consumable_slot2_icon != null)
        {
            Sprite s2 = GetConsumableSprite(c2_name);
            consumable_slot2_icon.sprite = s2;
            consumable_slot2_icon.color = s2 != null ? Color.white : Color.clear;
            consumable_slot2_icon.enabled = s2 != null;
        }

        if (consumable_slot1_bg != null)
        {
            consumable_slot1_bg.color = active_slot == 1 ? slot_active_color : slot_inactive_color;
        }

        if (consumable_slot2_bg != null)
        {
            consumable_slot2_bg.color = active_slot == 2 ? slot_active_color : slot_inactive_color;
        }
    }

    public Sprite GetConsumableSprite(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        if (itemName == "Soap Bomb") return icon_soap_bomb;
        if (itemName == "First Aid Kit") return icon_first_aid;
        if (itemName == "Tools Box" || itemName == "Tool Box") return icon_tools_box;
        if (itemName == "Energy Drink") return icon_energy_drink;
        if (itemName == "Soap Water Bucket" || itemName == "Soap Bucket") return icon_soap_bucket;
        if (itemName == "Sonar Batarang") return icon_sonar_batarang;
        return null;
    }

    private void UpdateMinimarketUI()
    {
        if (Minimarket.instance != null && Minimarket.instance.health != null && minimarket_health_slider != null)
        {
            minimarket_health_slider.maxValue = Minimarket.instance.health.health_max;
            minimarket_health_slider.value = Minimarket.instance.health.health_current;
        }
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (planning_panel != null)
        {
            planning_panel.SetActive(phase == GamePhase.Planning);
        }
    }

    private void OnWaveStarted(int current_wave, int total_waves)
    {
        if (break_timer_text != null)
        {
            break_timer_text.gameObject.SetActive(false);
        }

        if (wave_text != null)
        {
            wave_text.text = "Wave " + current_wave + " / " + total_waves;
        }

        if (wave_progress_slider != null)
        {
            wave_progress_slider.minValue = 0;
            wave_progress_slider.maxValue = total_waves;
            wave_progress_slider.wholeNumbers = true;
            wave_progress_slider.value = current_wave;
        }
    }

    private void OnBreakStarted(float duration)
    {
        if (break_timer_text != null)
        {
            break_timer_text.gameObject.SetActive(true);
            break_timer_text.text = "Next Wave in: " + Mathf.CeilToInt(duration) + "s";
        }
    }

    private void OnBreakTimerTick(float remaining)
    {
        if (break_timer_text != null)
        {
            if (remaining > 0f)
            {
                break_timer_text.gameObject.SetActive(true);
                break_timer_text.text = "Next Wave in: " + Mathf.CeilToInt(remaining) + "s";
            }
            else
            {
                break_timer_text.gameObject.SetActive(false);
            }
        }
    }

    private void OnBreakEnded()
    {
        if (break_timer_text != null)
        {
            break_timer_text.gameObject.SetActive(false);
        }
    }

    private void OnHugeWaveAlert(bool is_huge)
    {
        if (huge_wave_banner != null)
        {
            huge_wave_banner.SetActive(true);
            Invoke(nameof(HideHugeWaveBanner), 3f);
        }
    }

    private void HideHugeWaveBanner()
    {
        if (huge_wave_banner != null) huge_wave_banner.SetActive(false);
    }

    private void OnDefensePointsChanged(int current, int max)
    {
        if (defense_points_text != null)
        {
            defense_points_text.text = "Defense Points: " + current + " / " + max;
        }

        UpdateDefenseButtonStates(current);
    }

    private void UpdateDefenseButtonStates(int current_points)
    {
        if (DefensePlacementManager.instance == null) return;
        var defs = DefensePlacementManager.instance.available_defenses;
        if (defense_btn_fence != null && defs.Count > 0) defense_btn_fence.interactable = current_points >= defs[0].point_cost;
        if (defense_btn_cannon != null && defs.Count > 1) defense_btn_cannon.interactable = current_points >= defs[1].point_cost;
        if (defense_btn_trap != null && defs.Count > 2) defense_btn_trap.interactable = current_points >= defs[2].point_cost;
        if (defense_btn_barrel != null && defs.Count > 3) defense_btn_barrel.interactable = current_points >= defs[3].point_cost;
    }

    private void OnDefenseSelectionChanged(DefenseData selected, bool is_refund)
    {
        SetButtonOutline(defense_btn_fence, selected != null && selected.defense_name.Contains("Fence"));
        SetButtonOutline(defense_btn_cannon, selected != null && selected.defense_name.Contains("Cannon"));
        SetButtonOutline(defense_btn_trap, selected != null && selected.defense_name.Contains("Balloon"));
        SetButtonOutline(defense_btn_barrel, selected != null && selected.defense_name.Contains("Barrel"));
        SetButtonOutline(defense_btn_refund, is_refund);
    }

    private void SetButtonOutline(Button btn, bool is_selected)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = is_selected ? defense_btn_selected_color : defense_btn_normal_color;
        }
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

    public void ShowPickupFeedback(string message, Color color)
    {
        if (pickup_feedback_text == null || pickup_feedback_group == null) return;

        pickup_feedback_text.text = message;
        pickup_feedback_text.color = color;

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

    private void OnVictory(int stars)
    {
        Time.timeScale = 0f;
        if (victory_panel != null)
        {
            victory_panel.SetActive(true);
            if (victory_stars_text != null)
            {
                victory_stars_text.text = "Stars: " + stars + " / 3";
            }
        }
    }

    private void OnDefeat()
    {
        Time.timeScale = 0f;
        if (defeat_panel != null)
        {
            defeat_panel.SetActive(true);
        }
    }

    public void TogglePause()
    {
        if (settings_panel != null && settings_panel.gameObject.activeSelf)
        {
            settings_panel.Close();
            return;
        }

        if (pause_panel == null) return;
        bool is_paused = !pause_panel.activeSelf;
        pause_panel.SetActive(is_paused);
        Time.timeScale = is_paused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        if (settings_panel != null) settings_panel.gameObject.SetActive(false);
        if (pause_panel != null) pause_panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
