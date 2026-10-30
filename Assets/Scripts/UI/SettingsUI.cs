using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

[Serializable]
public class KeybindRow
{
    public KeyAction action;
    public string display_name;
    public Button rebind_button;
    public TMP_Text key_text;
}

public class SettingsUI : MonoBehaviour
{
    [Header("Modal Animation")]
    [SerializeField] private RectTransform modal_box;

    [Header("Tab Navigation")]
    [SerializeField] private Button tab_audio_btn;
    [SerializeField] private Button tab_controls_btn;
    [SerializeField] private GameObject audio_panel;
    [SerializeField] private GameObject controls_panel;

    [Header("Audio Settings")]
    [SerializeField] private Slider music_volume_slider;
    [SerializeField] private Slider sfx_volume_slider;

    [Header("Controls Settings")]
    [SerializeField] private List<KeybindRow> keybind_rows = new List<KeybindRow>();
    [SerializeField] private Button reset_controls_btn;

    [Header("Navigation")]
    [SerializeField] private Button back_button;

    public event Action on_closed;

    private Coroutine rebind_coroutine;
    private KeybindRow current_rebinding_row;

    private void Awake()
    {
        if (tab_audio_btn != null) tab_audio_btn.onClick.AddListener(() => SwitchTab(0));
        if (tab_controls_btn != null) tab_controls_btn.onClick.AddListener(() => SwitchTab(1));

        if (music_volume_slider != null)
        {
            music_volume_slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfx_volume_slider != null)
        {
            sfx_volume_slider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (reset_controls_btn != null)
        {
            reset_controls_btn.onClick.AddListener(OnResetControlsClicked);
        }

        if (back_button != null)
        {
            back_button.onClick.AddListener(Close);
        }

        SetupKeybindButtons();
    }

    private void OnEnable()
    {
        RefreshAudioSliders();
        RefreshKeybindLabels();
        SwitchTab(0);
        AnimateOpen();
    }

    private void OnDisable()
    {
        if (rebind_coroutine != null)
        {
            StopCoroutine(rebind_coroutine);
            rebind_coroutine = null;
        }
        if (current_rebinding_row != null)
        {
            RefreshKeybindLabels();
            current_rebinding_row = null;
        }
    }

    public void SwitchTab(int index)
    {
        if (audio_panel != null) audio_panel.SetActive(index == 0);
        if (controls_panel != null) controls_panel.SetActive(index == 1);

        if (tab_audio_btn != null)
        {
            var img = tab_audio_btn.GetComponent<Image>();
            if (img != null) img.color = index == 0 ? new Color(0.2f, 0.7f, 0.9f, 0.9f) : new Color(0.15f, 0.17f, 0.25f, 0.8f);
        }

        if (tab_controls_btn != null)
        {
            var img = tab_controls_btn.GetComponent<Image>();
            if (img != null) img.color = index == 1 ? new Color(0.2f, 0.7f, 0.9f, 0.9f) : new Color(0.15f, 0.17f, 0.25f, 0.8f);
        }
    }

    private void RefreshAudioSliders()
    {
        if (music_volume_slider != null)
        {
            float max = music_volume_slider.maxValue > 0 ? music_volume_slider.maxValue : 1f;
            float normalized = PlayerPrefs.GetFloat(GameConstants.PrefKeys.MusicVolume, 0.8f);
            music_volume_slider.value = normalized * max;
        }
        if (sfx_volume_slider != null)
        {
            float max = sfx_volume_slider.maxValue > 0 ? sfx_volume_slider.maxValue : 1f;
            float normalized = PlayerPrefs.GetFloat(GameConstants.PrefKeys.SfxVolume, 1f);
            sfx_volume_slider.value = normalized * max;
        }
    }

    private void OnMusicVolumeChanged(float val)
    {
        float max = music_volume_slider != null && music_volume_slider.maxValue > 0 ? music_volume_slider.maxValue : 1f;
        float normalized = Mathf.Clamp01(val / max);
        AudioManager.SetMusicVolume(normalized);
        PlayerPrefs.SetFloat(GameConstants.PrefKeys.MusicVolume, normalized);
        PlayerPrefs.Save();
    }

    private void OnSfxVolumeChanged(float val)
    {
        float max = sfx_volume_slider != null && sfx_volume_slider.maxValue > 0 ? sfx_volume_slider.maxValue : 1f;
        float normalized = Mathf.Clamp01(val / max);
        AudioManager.SetSFXVolume(normalized);
        PlayerPrefs.SetFloat(GameConstants.PrefKeys.SfxVolume, normalized);
        PlayerPrefs.Save();
    }

    private void SetupKeybindButtons()
    {
        foreach (var row in keybind_rows)
        {
            if (row.rebind_button != null)
            {
                var targetRow = row;
                row.rebind_button.onClick.AddListener(() => StartRebinding(targetRow));
            }
        }
    }

    public void RefreshKeybindLabels()
    {
        foreach (var row in keybind_rows)
        {
            if (row.key_text != null)
            {
                ActionBinding b = KeybindManager.GetBinding(row.action);
                row.key_text.text = b.ToString();
            }
        }
    }

    private void StartRebinding(KeybindRow row)
    {
        if (rebind_coroutine != null)
        {
            StopCoroutine(rebind_coroutine);
            RefreshKeybindLabels();
        }

        current_rebinding_row = row;
        if (row.key_text != null)
        {
            row.key_text.text = "...";
        }

        rebind_coroutine = StartCoroutine(WaitForKeyPressCoroutine(row));
    }

    private IEnumerator WaitForKeyPressCoroutine(KeybindRow row)
    {
        yield return null; // Wait one frame to avoid capturing current button click

        while (true)
        {
            // Check Mouse
            Mouse m = Mouse.current;
            if (m != null)
            {
                if (m.leftButton.wasPressedThisFrame)
                {
                    KeybindManager.SetBinding(row.action, new ActionBinding(0));
                    break;
                }
                if (m.rightButton.wasPressedThisFrame)
                {
                    KeybindManager.SetBinding(row.action, new ActionBinding(1));
                    break;
                }
                if (m.middleButton.wasPressedThisFrame)
                {
                    KeybindManager.SetBinding(row.action, new ActionBinding(2));
                    break;
                }
            }

            // Check Keyboard
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    // Cancel rebinding on Escape
                    break;
                }

                bool found = false;
                foreach (var keyControl in kb.allKeys)
                {
                    if (keyControl.wasPressedThisFrame)
                    {
                        KeybindManager.SetBinding(row.action, new ActionBinding(keyControl.keyCode));
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            yield return null;
        }

        RefreshKeybindLabels();
        current_rebinding_row = null;
        rebind_coroutine = null;
    }

    private void OnResetControlsClicked()
    {
        KeybindManager.ResetToDefaults();
        RefreshKeybindLabels();
    }

    private RectTransform GetModalBox()
    {
        if (modal_box != null) return modal_box;
        Transform child = transform.Find("SettingsModal");
        if (child != null) modal_box = child.GetComponent<RectTransform>();
        return modal_box;
    }

    private void AnimateOpen()
    {
        RectTransform target = GetModalBox();
        if (target != null)
        {
            target.DOKill();
            target.anchoredPosition = new Vector2(0f, 900f);
            target.DOAnchorPos(Vector2.zero, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    public void Close()
    {
        RectTransform target = GetModalBox();
        if (target != null)
        {
            target.DOKill();
            target.DOAnchorPos(new Vector2(0f, 900f), 0.25f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    on_closed?.Invoke();
                });
        }
        else
        {
            gameObject.SetActive(false);
            on_closed?.Invoke();
        }
    }
}
