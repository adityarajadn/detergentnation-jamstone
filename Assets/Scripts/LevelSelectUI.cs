using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Top Bar")]
    public Text candyText;
    public Button backButton;

    [Header("Sprites")]
    public Sprite starFilled;
    public Sprite starEmpty;
    public Sprite cardFrame;

    [Header("Cards Root")]
    public Transform cardsParent;

    [Serializable]
    public class LevelCardDef
    {
        public int levelId;
        public string title;
        public int defensePts;
        public int totalWaves;
    }

    private readonly LevelCardDef[] levels = new LevelCardDef[]
    {
        new LevelCardDef { levelId = 1, title = "Level 1: First Splash", defensePts = 6, totalWaves = 4 },
        new LevelCardDef { levelId = 2, title = "Level 2: Marsh Swarm", defensePts = 9, totalWaves = 4 },
        new LevelCardDef { levelId = 3, title = "Level 3: Sweet Assault", defensePts = 10, totalWaves = 5 },
        new LevelCardDef { levelId = 4, title = "Level 4: Cheesy Siege", defensePts = 10, totalWaves = 7 },
        new LevelCardDef { levelId = 5, title = "Level 5: Final Stand", defensePts = 12, totalWaves = 10 }
    };

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        RefreshAll();
    }

    private void Update()
    {
        if (candyText != null && GameState.Instance != null)
        {
            candyText.text = $"CANDY: {GameState.Instance.data.candy}";
        }
    }

    public void RefreshAll()
    {
        if (GameState.Instance == null) return;
        var data = GameState.Instance.data;

        if (candyText != null)
        {
            candyText.text = $"CANDY: {data.candy}";
        }

        if (cardsParent == null) return;

        int childCount = cardsParent.childCount;
        for (int i = 0; i < levels.Length && i < childCount; i++)
        {
            var def = levels[i];
            Transform card = cardsParent.GetChild(i);
            UpdateCard(card, def, data);
        }
    }

    private void UpdateCard(Transform card, LevelCardDef def, SaveData data)
    {
        // 1. Title
        var titleTxt = card.Find("Title")?.GetComponent<Text>();
        if (titleTxt != null) titleTxt.text = def.title;

        // 2. Stars
        int earnedStars = 0;
        if (def.levelId >= 0 && def.levelId < data.levelStars.Length)
        {
            earnedStars = data.levelStars[def.levelId];
        }

        var starsTr = card.Find("Stars");
        if (starsTr != null)
        {
            for (int s = 0; s < 3 && s < starsTr.childCount; s++)
            {
                var starImg = starsTr.GetChild(s).GetComponent<UnityEngine.UI.Image>();
                if (starImg != null)
                {
                    starImg.sprite = (s < earnedStars) ? starFilled : starEmpty;
                }
            }
        }

        // 3. Stats
        var defPtsTxt = card.Find("DefensePts")?.GetComponent<Text>();
        if (defPtsTxt != null) defPtsTxt.text = $"Defense Pts: {def.defensePts}";

        var wavesTxt = card.Find("TotalWaves")?.GetComponent<Text>();
        if (wavesTxt != null) wavesTxt.text = $"Total Waves: {def.totalWaves}";

        // 4. Play / Locked Button
        bool isUnlocked = (def.levelId <= data.unlockedLevels);
        var playBtn = card.Find("ActionBtn")?.GetComponent<Button>();
        var btnImg = playBtn?.GetComponent<UnityEngine.UI.Image>();
        var btnTxt = playBtn?.GetComponentInChildren<Text>();

        if (playBtn != null && btnImg != null && btnTxt != null)
        {
            playBtn.onClick.RemoveAllListeners();

            if (isUnlocked)
            {
                btnTxt.text = "PLAY";
                btnTxt.color = Color.white;
                btnImg.color = new Color(0.01f, 0.41f, 0.63f, 1f); // #0269a1 cyan button
                playBtn.interactable = true;
                int lvl = def.levelId;
                playBtn.onClick.AddListener(() => OnPlayLevel(lvl));
            }
            else
            {
                btnTxt.text = "LOCKED";
                btnTxt.color = new Color(0.35f, 0.42f, 0.52f, 1f); // dim gray
                btnImg.color = new Color(0.05f, 0.08f, 0.13f, 0.85f); // dark locked
                playBtn.interactable = false;
            }
        }
    }

    public void OnPlayLevel(int levelId)
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.data.currentLevel = levelId;
            GameState.Instance.SaveGame();
        }
        SceneManager.LoadScene("LevelBase");
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
