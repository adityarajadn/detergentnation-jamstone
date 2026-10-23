using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button shopButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Currency Display")]
    public Text candyText;

    [Header("Settings Tabs")]
    public GameObject settingsPanel;
    public Button tabAudioBtn;
    public Button tabKeybindsBtn;
    public GameObject tabAudioContent;
    public GameObject tabKeybindsContent;
    public Button settingsCloseBtn;

    [Header("Audio Settings")]
    public Slider masterVolSlider;
    public Slider musicVolSlider;
    public Slider sfxVolSlider;

    [Header("Keybinds")]
    public Button bindMoveLeftBtn;
    public Text bindMoveLeftText;
    public Button bindMoveRightBtn;
    public Text bindMoveRightText;
    public Button bindJumpBtn;
    public Text bindJumpText;
    public Button bindItemBtn;
    public Text bindItemText;
    public Button bindInteractBtn;
    public Text bindInteractText;
    public Button resetKeybindsBtn;
    public GameObject rebindPrompt;

    [Header("Credits Panel")]
    public GameObject creditsPanel;
    public Button creditsCloseBtn;

    private string currentRebindingAction = null;

    private void Awake()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfx = PlayerPrefs.GetFloat("SfxVolume", 1.0f);

        AudioListener.volume = master;
        if (masterVolSlider != null) masterVolSlider.value = master;
        if (musicVolSlider != null) musicVolSlider.value = music;
        if (sfxVolSlider != null) sfxVolSlider.value = sfx;

        if (GameState.Instance != null)
        {
            GameState.Instance.data.volumeMaster = master;
            GameState.Instance.data.volumeBgm = music;
            GameState.Instance.data.volumeSfx = sfx;
        }
    }

    private void Start()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (shopButton != null) shopButton.onClick.AddListener(OnShopClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        if (settingsCloseBtn != null) settingsCloseBtn.onClick.AddListener(ClosePanels);
        if (creditsCloseBtn != null) creditsCloseBtn.onClick.AddListener(ClosePanels);

        // Tab Switching
        if (tabAudioBtn != null) tabAudioBtn.onClick.AddListener(() => SwitchTab(true));
        if (tabKeybindsBtn != null) tabKeybindsBtn.onClick.AddListener(() => SwitchTab(false));

        // Volume Sliders
        if (masterVolSlider != null) masterVolSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolSlider != null) musicVolSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolSlider != null) sfxVolSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // Keybind Buttons
        if (bindMoveLeftBtn != null) bindMoveLeftBtn.onClick.AddListener(() => StartRebinding("Key_MoveLeft"));
        if (bindMoveRightBtn != null) bindMoveRightBtn.onClick.AddListener(() => StartRebinding("Key_MoveRight"));
        if (bindJumpBtn != null) bindJumpBtn.onClick.AddListener(() => StartRebinding("Key_Jump"));
        if (bindItemBtn != null) bindItemBtn.onClick.AddListener(() => StartRebinding("Key_UseItem"));
        if (bindInteractBtn != null) bindInteractBtn.onClick.AddListener(() => StartRebinding("Key_Interact"));
        if (resetKeybindsBtn != null) resetKeybindsBtn.onClick.AddListener(ResetKeybinds);

        KeybindManager.OnKeybindsChanged += RefreshKeybindTexts;
        RefreshKeybindTexts();

        ClosePanels();
        UpdateCandy();
    }

    private void OnDestroy()
    {
        KeybindManager.OnKeybindsChanged -= RefreshKeybindTexts;
    }

    private void Update()
    {
        UpdateCandy();

        if (currentRebindingAction != null)
        {
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k))
                {
                    if (k != KeyCode.Escape)
                    {
                        KeybindManager.SetKey(currentRebindingAction, k);
                    }
                    currentRebindingAction = null;
                    if (rebindPrompt != null) rebindPrompt.SetActive(false);
                    break;
                }
            }
        }
    }

    public void SwitchTab(bool isAudio)
    {
        if (tabAudioContent != null) tabAudioContent.SetActive(isAudio);
        if (tabKeybindsContent != null) tabKeybindsContent.SetActive(!isAudio);

        if (tabAudioBtn != null)
            tabAudioBtn.GetComponent<Image>().color = isAudio ? new Color(0.3f, 0.6f, 0.9f) : new Color(0.2f, 0.25f, 0.35f);
        if (tabKeybindsBtn != null)
            tabKeybindsBtn.GetComponent<Image>().color = !isAudio ? new Color(0.3f, 0.6f, 0.9f) : new Color(0.2f, 0.25f, 0.35f);
    }

    public void StartRebinding(string actionKey)
    {
        currentRebindingAction = actionKey;
        if (rebindPrompt != null) rebindPrompt.SetActive(true);
    }

    public void ResetKeybinds()
    {
        KeybindManager.ResetDefaults();
    }

    public void RefreshKeybindTexts()
    {
        if (bindMoveLeftText != null) bindMoveLeftText.text = KeybindManager.MoveLeft.ToString();
        if (bindMoveRightText != null) bindMoveRightText.text = KeybindManager.MoveRight.ToString();
        if (bindJumpText != null) bindJumpText.text = KeybindManager.Jump.ToString();
        if (bindItemText != null) bindItemText.text = KeybindManager.UseItem.ToString();
        if (bindInteractText != null) bindInteractText.text = KeybindManager.Interact.ToString();
    }

    public void OnMasterVolumeChanged(float val)
    {
        AudioListener.volume = val;
        PlayerPrefs.SetFloat("MasterVolume", val);
        PlayerPrefs.Save();
        if (GameState.Instance != null) GameState.Instance.data.volumeMaster = val;
    }

    public void OnMusicVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat("MusicVolume", val);
        PlayerPrefs.Save();
        if (GameState.Instance != null) GameState.Instance.data.volumeBgm = val;
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null)
        {
            AudioManager.Instance.bgmSource.volume = val * AudioListener.volume;
        }
    }

    public void OnSfxVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat("SfxVolume", val);
        PlayerPrefs.Save();
        if (GameState.Instance != null) GameState.Instance.data.volumeSfx = val;
    }

    private void UpdateCandy()
    {
        if (candyText != null && GameState.Instance != null)
        {
            candyText.text = $"Candy: {GameState.Instance.data.candy}";
        }
    }

    public void OnPlayClicked() => SceneManager.LoadScene("LevelSelect");
    public void OnShopClicked() => SceneManager.LoadScene("ShopScreen");

    public void OpenSettings()
    {
        ClosePanels();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            SwitchTab(true);
        }
    }

    public void OpenCredits()
    {
        ClosePanels();
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void ClosePanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (rebindPrompt != null) rebindPrompt.SetActive(false);
        currentRebindingAction = null;
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
