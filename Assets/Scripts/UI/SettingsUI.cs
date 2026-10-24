using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider music_volume_slider;
    [SerializeField] private Slider sfx_volume_slider;
    [SerializeField] private Button back_button;

    private void Start()
    {
        if (music_volume_slider != null)
        {
            music_volume_slider.value = PlayerPrefs.GetFloat(GameConstants.PrefKeys.MusicVolume, 0.8f);
            music_volume_slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfx_volume_slider != null)
        {
            sfx_volume_slider.value = PlayerPrefs.GetFloat(GameConstants.PrefKeys.SfxVolume, 1f);
            sfx_volume_slider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (back_button != null)
        {
            back_button.onClick.AddListener(SceneLoader.LoadMainMenu);
        }
    }

    private void OnMusicVolumeChanged(float val)
    {
        AudioManager.SetMusicVolume(val);
        PlayerPrefs.SetFloat(GameConstants.PrefKeys.MusicVolume, val);
        PlayerPrefs.Save();
    }

    private void OnSfxVolumeChanged(float val)
    {
        AudioManager.SetSFXVolume(val);
        PlayerPrefs.SetFloat(GameConstants.PrefKeys.SfxVolume, val);
        PlayerPrefs.Save();
    }
}
