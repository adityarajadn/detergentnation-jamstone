using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DefensePlacementManager : MonoBehaviour
{
    public static DefensePlacementManager instance;

    public int defense_points_max = 6;
    public int defense_points_current = 6;
    public List<DefenseData> available_defenses = new List<DefenseData>();
    public DefenseData selected_defense;
    public bool is_refund_mode = false;
    public UnityEvent<int, int> on_points_changed;
    public UnityEvent<DefenseData, bool> on_selection_changed;

    private int points_spent_total = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        HandleShortcuts();
    }

    private void HandleShortcuts()
    {
        UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame && available_defenses.Count > 0) SelectDefense(available_defenses[0]);
        if (kb.digit2Key.wasPressedThisFrame && available_defenses.Count > 1) SelectDefense(available_defenses[1]);
        if (kb.digit3Key.wasPressedThisFrame && available_defenses.Count > 2) SelectDefense(available_defenses[2]);
        if (kb.digit4Key.wasPressedThisFrame && available_defenses.Count > 3) SelectDefense(available_defenses[3]);
        if (kb.xKey.wasPressedThisFrame) ToggleRefundMode();
        if (kb.escapeKey.wasPressedThisFrame) ClearSelection();
    }

    public void SelectDefense(DefenseData data)
    {
        if (selected_defense == data && !is_refund_mode)
        {
            ClearSelection();
            return;
        }

        selected_defense = data;
        is_refund_mode = false;
        if (on_selection_changed != null)
        {
            on_selection_changed.Invoke(selected_defense, is_refund_mode);
        }
    }

    public void ToggleRefundMode()
    {
        is_refund_mode = !is_refund_mode;
        if (is_refund_mode) selected_defense = null;
        if (on_selection_changed != null)
        {
            on_selection_changed.Invoke(selected_defense, is_refund_mode);
        }
    }

    public void ClearSelection()
    {
        selected_defense = null;
        is_refund_mode = false;
        if (on_selection_changed != null)
        {
            on_selection_changed.Invoke(selected_defense, is_refund_mode);
        }
    }

    public void InitPoints(int starting_points)
    {
        defense_points_max = starting_points;
        defense_points_current = starting_points;
        points_spent_total = 0;
        if (on_points_changed != null)
        {
            on_points_changed.Invoke(defense_points_current, defense_points_max);
        }
    }

    public bool CanAfford(DefenseData data)
    {
        if (data == null) return false;
        return defense_points_current >= data.point_cost;
    }

    public GameObject PlaceDefense(DefenseData data, Vector3 position, Transform parent = null)
    {
        if (!CanAfford(data) || data.prefab == null) return null;

        defense_points_current -= data.point_cost;
        points_spent_total += data.point_cost;

        GameObject obj = Instantiate(data.prefab, position, Quaternion.identity, parent);
        DefenseObject defense = obj.GetComponent<DefenseObject>();
        if (defense != null)
        {
            defense.defense_data = data;
        }

        if (on_points_changed != null)
        {
            on_points_changed.Invoke(defense_points_current, defense_points_max);
        }

        return obj;
    }

    public void RefundDefense(DefenseData data)
    {
        if (data == null) return;
        defense_points_current += data.point_cost;
        if (defense_points_current > defense_points_max) defense_points_current = defense_points_max;

        if (on_points_changed != null)
        {
            on_points_changed.Invoke(defense_points_current, defense_points_max);
        }
    }

    public int GetTotalPointsSpent()
    {
        return points_spent_total;
    }
}
