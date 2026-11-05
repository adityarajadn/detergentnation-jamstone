using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Credits scene controller. Only wires the back button to the main menu.
/// </summary>
public class CreditsUI : MonoBehaviour
{
    [SerializeField] private Button back_button;

    private void Start()
    {
        if (back_button != null)
        {
            back_button.onClick.AddListener(OnBackClicked);
        }

        AudioManager.PlayMusic(GameConstants.AudioKeys.MusicMainMenu);
    }

    private void OnBackClicked()
    {
        AudioManager.PlaySFX(GameConstants.AudioKeys.UiClick);
        SceneLoader.LoadMainMenu();
    }
}
