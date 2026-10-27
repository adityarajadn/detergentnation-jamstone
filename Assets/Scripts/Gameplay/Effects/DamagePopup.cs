using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private static GameObject cached_prefab;

    [SerializeField] private TMP_Text text_mesh;
    [SerializeField] private float lifetime = 0.55f;
    [SerializeField] private float move_speed_y = 1.4f;

    private void Awake()
    {
        if (text_mesh == null)
        {
            text_mesh = GetComponentInChildren<TMP_Text>();
        }
    }

    public static DamagePopup Spawn(Vector3 world_position, int amount, Color? custom_color = null)
    {
        if (cached_prefab == null)
        {
            cached_prefab = Resources.Load<GameObject>("Effects/DamagePopup");
        }

        if (cached_prefab == null) return null;

        Vector3 spawn_pos = world_position + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.1f, 0.3f), 0f);
        GameObject obj = Instantiate(cached_prefab, spawn_pos, Quaternion.identity);
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        if (popup != null)
        {
            Color color = custom_color ?? new Color(1f, 0.88f, 0.32f, 1f); // Vibrant Yellow #FFE052
            popup.Setup(amount, color);
        }
        return popup;
    }

    public void Setup(int amount, Color color)
    {
        if (text_mesh != null)
        {
            text_mesh.text = amount.ToString();
            text_mesh.color = color;
        }

        StartCoroutine(AnimateRoutine(color));
    }

    private IEnumerator AnimateRoutine(Color base_color)
    {
        float elapsed = 0f;
        float vx = Random.Range(-0.4f, 0.4f);
        Vector3 initial_scale = transform.localScale;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Float upwards & drift
            float cur_vy = move_speed_y * (1f - t * 0.6f);
            transform.position += new Vector3(vx * Time.deltaTime, cur_vy * Time.deltaTime, 0f);

            // Punch scale
            float scale_factor;
            if (t < 0.2f)
            {
                scale_factor = Mathf.Lerp(0.6f, 1.3f, t / 0.2f);
            }
            else
            {
                scale_factor = Mathf.Lerp(1.3f, 1.0f, (t - 0.2f) / 0.8f);
            }
            transform.localScale = initial_scale * scale_factor;

            // Fade alpha in 2nd half
            if (t > 0.5f && text_mesh != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
                text_mesh.color = new Color(base_color.r, base_color.g, base_color.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
