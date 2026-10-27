using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button play_button;
    [SerializeField] private Button shop_button;
    [SerializeField] private Button settings_button;
    [SerializeField] private Button exit_button;
    [SerializeField] private Button credits_button;
    [SerializeField] private TMP_Text candy_text;
    [SerializeField] private GameObject settings_panel;
    [SerializeField] private GameObject credits_panel;

    private void Start()
    {
        if (play_button != null) play_button.onClick.AddListener(OnPlayClicked);
        if (shop_button != null) shop_button.onClick.AddListener(OnShopClicked);
        if (settings_button != null) settings_button.onClick.AddListener(OnSettingsClicked);
        if (exit_button != null) exit_button.onClick.AddListener(OnExitClicked);
        if (credits_button != null) credits_button.onClick.AddListener(OnCreditsClicked);
        if (settings_panel != null) settings_panel.SetActive(false);
        if (credits_panel != null) credits_panel.SetActive(false);

        UpdateCandyDisplay();
    }

    private void UpdateCandyDisplay()
    {
        if (candy_text != null)
        {
            int candies = GameManager.instance != null ? GameManager.instance.candy_total : PlayerPrefs.GetInt(GameConstants.PrefKeys.CandyTotal, 0);
            candy_text.text = "CANDY: " + candies;
        }
    }

    private void OnPlayClicked()
    {
        AudioEvents.TriggerGameStart();
        SceneLoader.LoadLevelSelect();
    }

    private void OnShopClicked()
    {
        AudioEvents.TriggerTitleSelect();
        SceneLoader.LoadShop();
    }

    private void OnSettingsClicked()
    {
        AudioEvents.TriggerTitleSelect();
        if (settings_panel != null)
        {
            settings_panel.SetActive(true);
        }
        else
        {
            SceneLoader.LoadSettings();
        }
    }

    private void OnCreditsClicked()
    {
        AudioEvents.TriggerTitleSelect();
        if (credits_panel != null)
        {
            credits_panel.SetActive(!credits_panel.activeSelf);
        }
    }

    private void OnExitClicked()
    {
        AudioEvents.TriggerTitleSelect();
        Application.Quit();
    }
}
