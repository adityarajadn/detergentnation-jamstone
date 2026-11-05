using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause panel and settings hand-off. Plain C# helper owned by GameplayUI.
/// </summary>
public class HudPauseMenu
{
    public GameObject pause_panel;
    public Button pause_resume_button;
    public Button pause_settings_button;
    public Button pause_menu_button;
    public SettingsUI settings_panel;

    public void Bind()
    {
        if (pause_resume_button != null)
        {
            pause_resume_button.onClick.AddListener(Resume);
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
            settings_panel.on_closed += OnSettingsClosed;
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
        if (pause_panel != null) pause_panel.SetActive(false);
    }

    public void Unbind()
    {
        if (settings_panel != null)
        {
            settings_panel.on_closed -= OnSettingsClosed;
        }
    }

    private void OnSettingsClosed()
    {
        if (pause_panel != null && Time.timeScale == 0f)
        {
            pause_panel.SetActive(true);
        }
    }

    public void Toggle()
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

    public void Resume()
    {
        if (settings_panel != null) settings_panel.gameObject.SetActive(false);
        if (pause_panel != null) pause_panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
