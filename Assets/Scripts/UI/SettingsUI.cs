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
            music_volume_slider.value = PlayerPrefs.GetFloat("music_volume", 0.8f);
            music_volume_slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfx_volume_slider != null)
        {
            sfx_volume_slider.value = PlayerPrefs.GetFloat("sfx_volume", 1f);
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
        PlayerPrefs.SetFloat("music_volume", val);
        PlayerPrefs.Save();
    }

    private void OnSfxVolumeChanged(float val)
    {
        AudioManager.SetSFXVolume(val);
        PlayerPrefs.SetFloat("sfx_volume", val);
        PlayerPrefs.Save();
    }
}
