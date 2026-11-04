using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Accordion expand/collapse animation for equipment rows in the shop.
/// Plain C# helper owned by ShopUI (no scene component).
/// </summary>
public class ShopAccordion
{
    private readonly MonoBehaviour host;
    private readonly List<EquipmentShopEntry> entries;
    private readonly float duration;

    private EquipmentShopEntry currently_expanded_entry = null;
    private Coroutine active_routine = null;

    public ShopAccordion(MonoBehaviour host, List<EquipmentShopEntry> entries, float duration)
    {
        this.host = host;
        this.entries = entries;
        this.duration = duration;
    }

    public void CollapseAll()
    {
        if (active_routine != null)
        {
            host.StopCoroutine(active_routine);
            active_routine = null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].expand_panel != null)
            {
                entries[i].expand_panel.SetActive(false);
                LayoutElement le = entries[i].expand_panel.GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredHeight = 0;
                }
            }
        }
        currently_expanded_entry = null;
    }

    public void Toggle(EquipmentShopEntry entry)
    {
        if (entry == null || entry.expand_panel == null) return;

        bool will_expand = (currently_expanded_entry != entry);

        if (active_routine != null)
        {
            host.StopCoroutine(active_routine);
            active_routine = null;
        }

        active_routine = host.StartCoroutine(AnimateRoutine(entry, will_expand));
    }

    private IEnumerator AnimateRoutine(EquipmentShopEntry targetEntry, bool will_expand)
    {
        EquipmentShopEntry closingEntry = currently_expanded_entry;
        EquipmentShopEntry openingEntry = will_expand ? targetEntry : null;

        // Calculate target height for opening entry
        float openingTargetHeight = 0f;
        LayoutElement openingLE = null;
        CanvasGroup openingCG = null;
        RectTransform openingRT = null;

        if (openingEntry != null && openingEntry.expand_panel != null)
        {
            openingEntry.expand_panel.SetActive(true);
            openingLE = openingEntry.expand_panel.GetComponent<LayoutElement>();
            openingCG = openingEntry.expand_panel.GetComponent<CanvasGroup>();
            openingRT = openingEntry.expand_panel.GetComponent<RectTransform>();

            // Calculate height by forcing layout calculation
            if (openingLE != null) openingLE.preferredHeight = -1; // unrestricted for measurement
            LayoutRebuilder.ForceRebuildLayoutImmediate(openingRT);
            openingTargetHeight = LayoutUtility.GetPreferredHeight(openingRT);
            if (openingTargetHeight <= 0f) openingTargetHeight = 65f * Mathf.Max(1, openingEntry.stat_upgrades.Count);
            if (openingLE != null) openingLE.preferredHeight = 0f;
            if (openingCG != null) openingCG.alpha = 0f;
        }

        // Setup closing entry
        float closingStartHeight = 0f;
        LayoutElement closingLE = null;
        CanvasGroup closingCG = null;

        if (closingEntry != null && closingEntry != openingEntry && closingEntry.expand_panel != null && closingEntry.expand_panel.activeSelf)
        {
            closingLE = closingEntry.expand_panel.GetComponent<LayoutElement>();
            closingCG = closingEntry.expand_panel.GetComponent<CanvasGroup>();
            closingStartHeight = closingLE != null && closingLE.preferredHeight > 0 ? closingLE.preferredHeight : LayoutUtility.GetPreferredHeight(closingEntry.expand_panel.GetComponent<RectTransform>());
        }

        currently_expanded_entry = openingEntry;

        Transform activePanelContent = targetEntry.expand_panel.transform.parent != null ? targetEntry.expand_panel.transform.parent.parent : null;
        RectTransform contentRT = activePanelContent != null ? activePanelContent.GetComponent<RectTransform>() : null;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (openingLE != null)
            {
                openingLE.preferredHeight = Mathf.Lerp(0f, openingTargetHeight, smoothT);
                if (openingCG != null) openingCG.alpha = smoothT;
            }

            if (closingLE != null)
            {
                closingLE.preferredHeight = Mathf.Lerp(closingStartHeight, 0f, smoothT);
                if (closingCG != null) closingCG.alpha = 1f - smoothT;
            }

            if (contentRT != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(contentRT);
            }

            yield return null;
        }

        // Finalize state
        if (openingLE != null)
        {
            openingLE.preferredHeight = openingTargetHeight;
            if (openingCG != null) openingCG.alpha = 1f;
        }

        if (closingLE != null && closingEntry != null && closingEntry.expand_panel != null)
        {
            closingLE.preferredHeight = 0f;
            if (closingCG != null) closingCG.alpha = 0f;
            closingEntry.expand_panel.SetActive(false);
        }

        if (contentRT != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
        }

        active_routine = null;
    }
}
