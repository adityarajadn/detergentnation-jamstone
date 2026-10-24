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
        SceneManager.LoadScene(SCENE_MAIN_MENU);
    }

    public static void LoadLevelSelect()
    {
        SceneManager.LoadScene(SCENE_LEVEL_SELECT);
    }

    public static void LoadGameplay(int level_number = 1)
    {
        GameManager.selected_level = level_number;
        SceneManager.LoadScene(SCENE_GAMEPLAY);
    }

    public static void LoadShop()
    {
        SceneManager.LoadScene(SCENE_SHOP);
    }

    public static void LoadSettings()
    {
        SceneManager.LoadScene(SCENE_SETTINGS);
    }
}
