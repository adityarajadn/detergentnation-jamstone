using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioSource sfx_source;
    [SerializeField] private AudioSource bgm_source;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (instance == null || clip == null || instance.sfx_source == null) return;
        instance.sfx_source.PlayOneShot(clip, volume);
    }

    public static void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (instance == null || instance.bgm_source == null) return;
        instance.bgm_source.clip = clip;
        instance.bgm_source.loop = loop;
        if (clip != null)
        {
            instance.bgm_source.Play();
        }
        else
        {
            instance.bgm_source.Stop();
        }
    }

    public static void SetSFXVolume(float volume)
    {
        if (instance == null || instance.sfx_source == null) return;
        instance.sfx_source.volume = volume;
    }

    public static void SetMusicVolume(float volume)
    {
        if (instance == null || instance.bgm_source == null) return;
        instance.bgm_source.volume = volume;
    }
}
