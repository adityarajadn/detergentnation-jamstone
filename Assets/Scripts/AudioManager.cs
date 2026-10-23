using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource bgmSource;
    public List<AudioSource> sfxPool = new List<AudioSource>();
    public Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < 8; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            sfxPool.Add(src);
        }
    }

    public void PlaySfx(string sfxName)
    {
        if (sfxClips.TryGetValue(sfxName, out var clip))
        {
            foreach (var src in sfxPool)
            {
                if (!src.isPlaying)
                {
                    src.volume = GameState.Instance != null ? GameState.Instance.data.volumeSfx * GameState.Instance.data.volumeMaster : 1.0f;
                    src.PlayOneShot(clip);
                    return;
                }
            }
        }
    }

    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = GameState.Instance != null ? GameState.Instance.data.volumeBgm * GameState.Instance.data.volumeMaster : 0.8f;
        bgmSource.Play();
    }

    public void StopBgm() => bgmSource?.Stop();
}
