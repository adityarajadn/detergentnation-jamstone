using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition instance { get; private set; }
    public static bool is_transitioning { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image transition_image;

    [Header("Sprite Sequence")]
    [SerializeField] private Sprite[] close_sprites;
    [SerializeField] private Sprite[] open_sprites;
    [SerializeField] private float frame_rate = 30f;

    private Coroutine current_routine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (transition_image != null)
        {
            transition_image.gameObject.SetActive(false);
        }
    }

    public static void LoadScene(string scene_name, Action on_complete = null)
    {
        EnsureReady();
        if (instance == null)
        {
            SceneManager.LoadScene(scene_name);
            on_complete?.Invoke();
            return;
        }

        if (is_transitioning) return;
        instance.StartTransition(scene_name, on_complete);
    }

    public static void LoadScene(int build_index, Action on_complete = null)
    {
        EnsureReady();
        if (instance == null)
        {
            SceneManager.LoadScene(build_index);
            on_complete?.Invoke();
            return;
        }

        if (is_transitioning) return;
        instance.StartTransition(build_index, on_complete);
    }

    private static void EnsureReady()
    {
        if (instance == null)
        {
            GlobalUIManager.EnsureInstance();
        }
    }

    private void StartTransition(string scene_name, Action on_complete)
    {
        if (current_routine != null) StopCoroutine(current_routine);
        current_routine = StartCoroutine(TransitionRoutine(() => SceneManager.LoadSceneAsync(scene_name), on_complete));
    }

    private void StartTransition(int build_index, Action on_complete)
    {
        if (current_routine != null) StopCoroutine(current_routine);
        current_routine = StartCoroutine(TransitionRoutine(() => SceneManager.LoadSceneAsync(build_index), on_complete));
    }

    private IEnumerator TransitionRoutine(Func<AsyncOperation> load_action, Action on_complete)
    {
        is_transitioning = true;
        float frame_delay = 1f / Mathf.Max(frame_rate, 1f);

        if (transition_image != null)
        {
            transition_image.gameObject.SetActive(true);
            transition_image.raycastTarget = true;
        }

        // 1. Close: Top to bottom (0 -> 14)
        if (close_sprites != null && transition_image != null)
        {
            for (int i = 0; i < close_sprites.Length; i++)
            {
                transition_image.sprite = close_sprites[i];
                yield return new WaitForSecondsRealtime(frame_delay);
            }
        }

        // 2. Load Scene Async
        AsyncOperation op = load_action();
        if (op != null)
        {
            while (!op.isDone)
            {
                yield return null;
            }
        }

        // Extra frame buffer for scene init
        yield return null;

        // 3. Open: Bottom to top (15 -> 29)
        if (open_sprites != null && transition_image != null)
        {
            for (int i = 0; i < open_sprites.Length; i++)
            {
                transition_image.sprite = open_sprites[i];
                yield return new WaitForSecondsRealtime(frame_delay);
            }
        }

        if (transition_image != null)
        {
            transition_image.gameObject.SetActive(false);
        }

        is_transitioning = false;
        current_routine = null;
        on_complete?.Invoke();
    }
}
