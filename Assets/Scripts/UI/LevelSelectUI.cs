using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private Button[] level_buttons;
    [SerializeField] private TMP_Text[] level_star_texts;
    [SerializeField] private Button back_button;

    private void Start()
    {
        if (back_button != null)
        {
            back_button.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        int unlocked = GameManager.instance != null ? GameManager.instance.highest_unlocked_level : 1;

        for (int i = 0; i < level_buttons.Length; i++)
        {
            int level_num = i + 1;
            bool is_unlocked = level_num <= unlocked;
            level_buttons[i].interactable = is_unlocked;

            if (is_unlocked)
            {
                level_buttons[i].onClick.AddListener(() =>
                {
                    SceneLoader.LoadGameplay(level_num);
                });
            }

            if (i < level_star_texts.Length && level_star_texts[i] != null)
            {
                int stars = GameManager.instance != null ? GameManager.instance.level_stars[i] : 0;
                level_star_texts[i].text = is_unlocked ? "Stars: " + stars + "/3" : "Locked";
            }
        }
    }
}
