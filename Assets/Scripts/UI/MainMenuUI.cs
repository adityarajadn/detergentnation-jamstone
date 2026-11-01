using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    private RectTransform credits_modal;
    private Button credits_back_button;

    private void Start()
    {
        if (play_button != null) play_button.onClick.AddListener(OnPlayClicked);
        if (shop_button != null) shop_button.onClick.AddListener(OnShopClicked);
        if (settings_button != null) settings_button.onClick.AddListener(OnSettingsClicked);
        if (exit_button != null) exit_button.onClick.AddListener(OnExitClicked);
        if (credits_button != null) credits_button.onClick.AddListener(OnCreditsClicked);
        if (settings_panel != null) settings_panel.SetActive(false);
        if (credits_panel != null)
        {
            SetupCreditsPanel();
            credits_panel.SetActive(false);
        }

        UpdateCandyDisplay();
        AudioManager.PlayMusic(GameConstants.AudioKeys.MusicMainMenu);
    }

    private void SetupCreditsPanel()
    {
        if (credits_panel == null) return;
        Transform modalTransform = credits_panel.transform.Find("CreditsModal");
        if (modalTransform != null)
        {
            credits_modal = modalTransform.GetComponent<RectTransform>();
            Transform backBtn = modalTransform.Find("UI_Button_Back");
            if (backBtn != null)
            {
                credits_back_button = backBtn.GetComponent<Button>();
                if (credits_back_button != null)
                {
                    credits_back_button.onClick.AddListener(CloseCredits);
                }
            }
        }
    }

    private void CloseCredits()
    {
        AudioManager.PlaySFX(GameConstants.AudioKeys.UiClick);
        if (credits_modal != null)
        {
            credits_modal.DOKill();
            credits_modal.DOAnchorPos(new Vector2(0f, 900f), 0.25f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (credits_panel != null) credits_panel.SetActive(false);
                });
        }
        else if (credits_panel != null)
        {
            credits_panel.SetActive(false);
        }
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
        AudioManager.PlaySFX(GameConstants.AudioKeys.GameStart);
        SceneLoader.LoadLevelSelect();
    }

    private void OnShopClicked()
    {
        AudioManager.PlaySFX(GameConstants.AudioKeys.UiClick);
        SceneLoader.LoadShop();
    }

    private void OnSettingsClicked()
    {
        AudioManager.PlaySFX(GameConstants.AudioKeys.UiClick);
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
        AudioManager.PlaySFX(GameConstants.AudioKeys.UiClick);
        if (credits_panel != null)
        {
            if (!credits_panel.activeSelf)
            {
                credits_panel.SetActive(true);
                if (credits_modal == null) SetupCreditsPanel();
                if (credits_modal != null)
                {
                    credits_modal.DOKill();
                    credits_modal.anchoredPosition = new Vector2(0f, 900f);
                    credits_modal.DOAnchorPos(Vector2.zero, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
                }
            }
            else
            {
                CloseCredits();
            }
        }
    }

    private void OnExitClicked()
    {
        AudioManager.PlaySFX(GameConstants.AudioKeys.UiClick);
        Application.Quit();
    }
}
