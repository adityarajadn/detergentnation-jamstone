using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Victory and defeat panels. Plain C# helper owned by GameplayUI.
/// </summary>
public class HudEndScreens
{
    public GameObject victory_panel;
    public TMP_Text victory_stars_text;
    public Button victory_next_button;
    public Button victory_retry_button;
    public Button victory_menu_button;

    public GameObject defeat_panel;
    public Button defeat_retry_button;
    public Button defeat_menu_button;

    public void Bind()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.on_victory.AddListener(OnVictory);
            LevelManager.instance.on_defeat.AddListener(OnDefeat);
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

        if (victory_panel != null) victory_panel.SetActive(false);
        if (defeat_panel != null) defeat_panel.SetActive(false);
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
}
