using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamagePopup : MonoBehaviour
{
    private static GameObject cached_prefab;
    private static ObjectPool<DamagePopup> pool;

    [SerializeField] private TMP_Text text_mesh;
    [SerializeField] private float lifetime = 0.55f;
    [SerializeField] private float move_speed_y = 1.4f;

    private static void EnsurePool()
    {
        if (pool == null)
        {
            pool = new ObjectPool<DamagePopup>(
                createFunc: () =>
                {
                    if (cached_prefab == null)
                    {
                        cached_prefab = Resources.Load<GameObject>("Effects/DamagePopup");
                    }
                    GameObject obj = Instantiate(cached_prefab);
                    return obj.GetComponent<DamagePopup>();
                },
                actionOnGet: popup =>
                {
                    popup.gameObject.SetActive(true);
                },
                actionOnRelease: popup =>
                {
                    popup.gameObject.SetActive(false);
                },
                actionOnDestroy: popup =>
                {
                    if (popup != null) Destroy(popup.gameObject);
                },
                defaultCapacity: 20,
                maxSize: 100
            );
        }
    }

    private void Awake()
    {
        if (text_mesh == null)
        {
            text_mesh = GetComponentInChildren<TMP_Text>();
        }
    }

    public static DamagePopup Spawn(Vector3 world_position, int amount)
    {
        return SpawnText(world_position, amount.ToString());
    }

    public static DamagePopup SpawnText(Vector3 world_position, string message)
    {
        if (cached_prefab == null)
        {
            cached_prefab = Resources.Load<GameObject>("Effects/DamagePopup");
        }

        if (cached_prefab == null) return null;

        EnsurePool();

        Vector3 spawn_pos = world_position + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.1f, 0.3f), 0f);
        DamagePopup popup = pool.Get();
        if (popup != null)
        {
            popup.transform.position = spawn_pos;
            popup.transform.rotation = Quaternion.identity;
            popup.Setup(message);
        }
        return popup;
    }

    public void Setup(int amount)
    {
        Setup(amount.ToString());
    }

    public void Setup(string message)
    {
        if (text_mesh != null)
        {
            text_mesh.text = message;
            text_mesh.alpha = 1f;
        }

        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        float elapsed = 0f;
        float vx = Random.Range(-0.4f, 0.4f);
        Vector3 initial_scale = Vector3.one;

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
                text_mesh.alpha = Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
            }

            yield return null;
        }

        transform.localScale = initial_scale;
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
