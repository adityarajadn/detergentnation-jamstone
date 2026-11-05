using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wave counter, huge-wave banner and break timer. Plain C# helper owned by GameplayUI.
/// </summary>
public class HudWaveView
{
    public TMP_Text wave_text;
    public GameObject huge_wave_banner;
    public TMP_Text break_timer_text;
    public Slider wave_progress_slider;

    private const float HugeWaveBannerSeconds = 3f;

    private readonly MonoBehaviour host;

    public HudWaveView(MonoBehaviour host)
    {
        this.host = host;
    }

    public void Bind()
    {
        if (huge_wave_banner != null) huge_wave_banner.SetActive(false);

        if (WaveManager.instance != null)
        {
            WaveManager.instance.on_wave_started.AddListener(OnWaveStarted);
            WaveManager.instance.on_huge_wave_alert.AddListener(OnHugeWaveAlert);
            WaveManager.instance.on_break_started.AddListener(OnBreakStarted);
            WaveManager.instance.on_break_timer_tick.AddListener(OnBreakTimerTick);
            WaveManager.instance.on_break_ended.AddListener(OnBreakEnded);
        }
    }

    private void OnWaveStarted(int current_wave, int total_waves)
    {
        if (break_timer_text != null)
        {
            break_timer_text.gameObject.SetActive(false);
        }

        if (wave_text != null)
        {
            wave_text.text = "Wave " + current_wave + " / " + total_waves;
        }

        if (wave_progress_slider != null)
        {
            wave_progress_slider.minValue = 0;
            wave_progress_slider.maxValue = total_waves;
            wave_progress_slider.wholeNumbers = true;
            wave_progress_slider.value = current_wave;
        }
    }

    private void OnBreakStarted(float duration)
    {
        if (break_timer_text != null)
        {
            break_timer_text.gameObject.SetActive(true);
            break_timer_text.text = "Next Wave in: " + Mathf.CeilToInt(duration) + "s";
        }
    }

    private void OnBreakTimerTick(float remaining)
    {
        if (break_timer_text != null)
        {
            if (remaining > 0f)
            {
                break_timer_text.gameObject.SetActive(true);
                break_timer_text.text = "Next Wave in: " + Mathf.CeilToInt(remaining) + "s";
            }
            else
            {
                break_timer_text.gameObject.SetActive(false);
            }
        }
    }

    private void OnBreakEnded()
    {
        if (break_timer_text != null)
        {
            break_timer_text.gameObject.SetActive(false);
        }
    }

    private void OnHugeWaveAlert(bool is_huge)
    {
        if (huge_wave_banner != null)
        {
            huge_wave_banner.SetActive(true);
            host.StartCoroutine(HideHugeWaveBannerAfterDelay());
        }
    }

    private IEnumerator HideHugeWaveBannerAfterDelay()
    {
        yield return new WaitForSeconds(HugeWaveBannerSeconds);
        if (huge_wave_banner != null) huge_wave_banner.SetActive(false);
    }
}
