using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Planning-phase panel: defense point counter, defense selection buttons, start-wave button.
/// Plain C# helper owned by GameplayUI.
/// </summary>
public class HudDefenseView
{
    public GameObject planning_panel;
    public TMP_Text defense_points_text;
    public Button start_wave_button;
    public Button defense_btn_fence;
    public Button defense_btn_cannon;
    public Button defense_btn_trap;
    public Button defense_btn_barrel;
    public Button defense_btn_refund;

    private readonly Color color_defense_normal = new Color(0.18f, 0.22f, 0.32f, 0.95f);
    private readonly Color color_defense_selected = new Color(0.15f, 0.58f, 0.92f, 1f);
    private readonly Color color_refund_normal = new Color(0.40f, 0.18f, 0.18f, 0.95f);
    private readonly Color color_refund_selected = new Color(0.85f, 0.22f, 0.22f, 1f);

    public void Bind()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.on_phase_changed.AddListener(OnPhaseChanged);
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
            OnDefenseSelectionChanged(DefensePlacementManager.instance.selected_defense, DefensePlacementManager.instance.is_refund_mode);
        }

        if (start_wave_button != null)
        {
            start_wave_button.onClick.AddListener(() =>
            {
                if (LevelManager.instance != null) LevelManager.instance.StartCombatPhase();
            });
        }
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (planning_panel != null)
        {
            planning_panel.SetActive(phase == GamePhase.Planning);
        }

        if (phase != GamePhase.Planning && DefensePlacementManager.instance != null)
        {
            DefensePlacementManager.instance.ClearSelection();
        }
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
        if (DefensePlacementManager.instance == null) return;
        var defs = DefensePlacementManager.instance.available_defenses;

        SetButtonSelected(defense_btn_fence, defs.Count > 0 && selected == defs[0], color_defense_normal, color_defense_selected);
        SetButtonSelected(defense_btn_cannon, defs.Count > 1 && selected == defs[1], color_defense_normal, color_defense_selected);
        SetButtonSelected(defense_btn_trap, defs.Count > 2 && selected == defs[2], color_defense_normal, color_defense_selected);
        SetButtonSelected(defense_btn_barrel, defs.Count > 3 && selected == defs[3], color_defense_normal, color_defense_selected);
        SetButtonSelected(defense_btn_refund, is_refund, color_refund_normal, color_refund_selected);
    }

    private void SetButtonSelected(Button btn, bool isSelected, Color normalColor, Color selectedColor)
    {
        if (btn == null) return;
        if (btn.image != null)
        {
            btn.image.color = isSelected ? selectedColor : normalColor;
        }
    }

}
