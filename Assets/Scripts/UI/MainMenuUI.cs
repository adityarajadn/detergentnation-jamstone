using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button play_button;
    [SerializeField] private Button shop_button;
    [SerializeField] private Button settings_button;
    [SerializeField] private Button exit_button;
    [SerializeField] private GameObject settings_panel;

    private void Start()
    {
        if (play_button != null) play_button.onClick.AddListener(OnPlayClicked);
        if (shop_button != null) shop_button.onClick.AddListener(OnShopClicked);
        if (settings_button != null) settings_button.onClick.AddListener(OnSettingsClicked);
        if (exit_button != null) exit_button.onClick.AddListener(OnExitClicked);
        if (settings_panel != null) settings_panel.SetActive(false);
    }

    private void OnPlayClicked()
    {
        SceneLoader.LoadLevelSelect();
    }

    private void OnShopClicked()
    {
        SceneLoader.LoadShop();
    }

    private void OnSettingsClicked()
    {
        if (settings_panel != null)
        {
            settings_panel.SetActive(true);
        }
        else
        {
            SceneLoader.LoadSettings();
        }
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }
}
