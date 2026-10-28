using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_LEVEL_SELECT = "LevelSelect";
    public const string SCENE_GAMEPLAY = "Gameplay";
    public const string SCENE_SHOP = "Shop";
    public const string SCENE_SETTINGS = "Settings";

    public static void LoadMainMenu()
    {
        AudioManager.PlaySFX("scene_transition");
        SceneTransition.LoadScene(SCENE_MAIN_MENU);
    }

    public static void LoadLevelSelect()
    {
        AudioManager.PlaySFX("scene_transition");
        SceneTransition.LoadScene(SCENE_LEVEL_SELECT);
    }

    public static void LoadGameplay(int level_number = 1)
    {
        AudioManager.PlaySFX("scene_transition");
        GameManager.selected_level = level_number;
        SceneTransition.LoadScene(SCENE_GAMEPLAY);
    }

    public static void LoadShop()
    {
        AudioManager.PlaySFX("scene_transition");
        SceneTransition.LoadScene(SCENE_SHOP);
    }

    public static void LoadSettings()
    {
        AudioManager.PlaySFX("scene_transition");
        SceneTransition.LoadScene(SCENE_SETTINGS);
    }
}
