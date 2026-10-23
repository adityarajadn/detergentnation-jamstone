using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    [Header("Health Bars")]
    public Slider playerHealthSlider;
    public Text playerHealthText;
    public Slider marketHealthSlider;
    public Text marketHealthText;

    [Header("Stats Display")]
    public Text ammoText;
    public Text waveText;
    public Text streakText;
    public Text candyText;
    public Text consumableText;

    [Header("Victory & Defeat Panels")]
    public GameObject victoryPanel;
    public Text victoryText;
    public Button victoryRestartBtn;
    public Button victoryMenuBtn;

    public GameObject defeatPanel;
    public Text defeatText;
    public Button defeatRestartBtn;
    public Button defeatMenuBtn;

    private Player player;
    private Minimarket minimarket;
    private WaveManager waveManager;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        minimarket = FindAnyObjectByType<Minimarket>();
        waveManager = FindAnyObjectByType<WaveManager>();

        if (player != null && player.health != null)
        {
            player.health.OnHealthChanged += UpdatePlayerHealth;
            UpdatePlayerHealth(player.health.healthCurrent, player.health.healthMax);
        }

        if (minimarket != null)
        {
            minimarket.OnHealthChanged += UpdateMarketHealth;
            UpdateMarketHealth(minimarket.healthCurrent, minimarket.healthMax);
        }

        if (waveManager != null)
        {
            waveManager.OnWaveStarted += (w, total, huge) => {
                if (waveText != null) waveText.text = $"Wave: {w}/{total}" + (huge ? " (HUGE WAVE!)" : "");
            };
            waveManager.OnKillStreakUpdated += (streak, bonus) => {
                if (streakText != null) streakText.text = streak > 1 ? $"Streak: {streak}x" + (bonus > 0 ? $" (+{bonus})" : "") : "";
            };
            waveManager.OnLevelCompleted += (stars) => ShowVictory(stars);
        }

        EventBus.OnLevelFailed += ShowDefeat;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (victoryRestartBtn != null) victoryRestartBtn.onClick.AddListener(OnRestartClicked);
        if (victoryMenuBtn != null) victoryMenuBtn.onClick.AddListener(OnMainMenuClicked);
        if (defeatRestartBtn != null) defeatRestartBtn.onClick.AddListener(OnRestartClicked);
        if (defeatMenuBtn != null) defeatMenuBtn.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        EventBus.OnLevelFailed -= ShowDefeat;
    }

    private void Update()
    {
        if (player != null && player.combat != null && ammoText != null)
        {
            ammoText.text = $"Ammo: {player.combat.clipCurrent}/{player.combat.clipMax} ({player.combat.carriedAmmo})";
        }

        if (candyText != null && GameState.Instance != null)
        {
            candyText.text = $"Candy: {GameState.Instance.data.candy}";
        }

        if (player != null && player.interaction != null && consumableText != null && GameState.Instance != null)
        {
            var list = GameState.Instance.data.equippedConsumables;
            string slot0 = list.Count > 0 ? $"{list[0]}: {GameState.Instance.GetConsumableQty(list[0])}" : "-";
            string slot1 = list.Count > 1 ? $"{list[1]}: {GameState.Instance.GetConsumableQty(list[1])}" : "-";
            int active = player.interaction.activeSlot;
            consumableText.text = $"Items: [1] {(active == 0 ? ">" : "")}{slot0}  [2] {(active == 1 ? ">" : "")}{slot1} (Q to use)";
        }
    }

    private void UpdatePlayerHealth(int current, int max)
    {
        if (playerHealthSlider != null) playerHealthSlider.value = (float)current / Mathf.Max(1, max);
        if (playerHealthText != null) playerHealthText.text = $"Player HP: {current}/{max}";
    }

    private void UpdateMarketHealth(int current, int max)
    {
        if (marketHealthSlider != null) marketHealthSlider.value = (float)current / Mathf.Max(1, max);
        if (marketHealthText != null) marketHealthText.text = $"RnA Mart: {current}/{max}";
    }

    public void ShowVictory(int stars)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryText != null) victoryText.text = $"VICTORY!\nStars Earned: {stars}/3";
        }
    }

    public void ShowDefeat(string reason)
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            if (defeatText != null) defeatText.text = $"DEFEAT\n{reason}";
        }
    }

    public void OnRestartClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void OnMainMenuClicked() => SceneManager.LoadScene("MainMenu");
    public void OnShopClicked() => SceneManager.LoadScene("ShopScreen");
}
