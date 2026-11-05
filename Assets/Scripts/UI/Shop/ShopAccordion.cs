using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Accordion expand/collapse animation for equipment rows in the shop.
/// Plain C# helper owned by ShopUI (no scene component).
/// </summary>
public class ShopAccordion
{
    private const float FallbackHeightPerUpgrade = 65f;

    private readonly List<EquipmentShopEntry> entries;
    private readonly float duration;

    private EquipmentShopEntry currently_expanded_entry = null;
    private Sequence active_sequence = null;

    public ShopAccordion(List<EquipmentShopEntry> entries, float duration)
    {
        this.entries = entries;
        this.duration = duration;
    }

    public void Kill()
    {
        if (active_sequence != null)
        {
            active_sequence.Kill();
            active_sequence = null;
        }
    }

    public void CollapseAll()
    {
        Kill();

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

        Kill();
        Animate(entry, will_expand);
    }

    private void Animate(EquipmentShopEntry targetEntry, bool will_expand)
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
            if (openingTargetHeight <= 0f) openingTargetHeight = FallbackHeightPerUpgrade * Mathf.Max(1, openingEntry.stat_upgrades.Count);
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

        // UI tween: keeps running while the game is paused (timeScale = 0)
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (openingLE != null)
        {
            LayoutElement le = openingLE;
            seq.Join(DOTween.To(() => le.preferredHeight, h => le.preferredHeight = h, openingTargetHeight, duration).SetEase(Ease.InOutSine));
            if (openingCG != null) seq.Join(openingCG.DOFade(1f, duration).SetEase(Ease.InOutSine));
        }

        if (closingLE != null)
        {
            LayoutElement le = closingLE;
            seq.Join(DOTween.To(() => le.preferredHeight, h => le.preferredHeight = h, 0f, duration).SetEase(Ease.InOutSine));
            if (closingCG != null) seq.Join(closingCG.DOFade(0f, duration).SetEase(Ease.InOutSine));
        }

        if (contentRT != null)
        {
            seq.OnUpdate(() => LayoutRebuilder.MarkLayoutForRebuild(contentRT));
        }

        seq.OnComplete(() =>
        {
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

            active_sequence = null;
        });

        active_sequence = seq;
    }
}
