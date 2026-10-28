using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [System.Serializable]
    public class SoundItem
    {
        public string sound_id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfx_source;
    [SerializeField] private AudioSource bgm_source;

    [Header("Audio Library")]
    [SerializeField] private List<SoundItem> audio_library = new List<SoundItem>();

    private readonly Dictionary<string, SoundItem> sound_map = new Dictionary<string, SoundItem>(System.StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSoundMap();
    }

    private void OnValidate()
    {
        BuildSoundMap();
    }

    public void BuildSoundMap()
    {
        sound_map.Clear();
        for (int i = 0; i < audio_library.Count; i++)
        {
            SoundItem item = audio_library[i];
            if (item != null && !string.IsNullOrEmpty(item.sound_id) && !sound_map.ContainsKey(item.sound_id))
            {
                sound_map.Add(item.sound_id, item);
            }
        }
    }

    public static void PlaySFX(string sound_id, float volume = 1f)
    {
        if (instance == null || string.IsNullOrEmpty(sound_id)) return;

        if (instance.sound_map.TryGetValue(sound_id, out SoundItem item) && item.clip != null)
        {
            PlaySFX(item.clip, item.volume * volume);
        }
    }

    public static void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (instance == null || clip == null || instance.sfx_source == null) return;
        instance.sfx_source.PlayOneShot(clip, volume);
    }

    public static void PlayMusic(string sound_id, bool loop = true)
    {
        if (instance == null || string.IsNullOrEmpty(sound_id)) return;

        if (instance.sound_map.TryGetValue(sound_id, out SoundItem item) && item.clip != null)
        {
            PlayMusic(item.clip, loop);
        }
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

    public static void StopMusic()
    {
        if (instance == null || instance.bgm_source == null) return;
        instance.bgm_source.Stop();
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
