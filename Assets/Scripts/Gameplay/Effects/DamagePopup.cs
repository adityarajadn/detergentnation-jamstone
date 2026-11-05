using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamagePopup : MonoBehaviour
{
    private const float DriftHeightFactor = 0.7f;
    private const float PunchStartScale = 0.6f;
    private const float PunchPeakScale = 1.3f;
    private const float PunchUpFraction = 0.2f;
    private const float FadeStartFraction = 0.5f;

    private static GameObject cached_prefab;
    private static ObjectPool<DamagePopup> pool;

    private Sequence sequence;

    [SerializeField] private TMP_Text text_mesh;
    [SerializeField] private float lifetime = 0.55f;
    [SerializeField] private float move_speed_y = 1.4f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        pool?.Clear();
        pool = null;
        cached_prefab = null;
    }

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
                    if (popup != null) popup.gameObject.SetActive(true);
                },
                actionOnRelease: popup =>
                {
                    if (popup != null) popup.gameObject.SetActive(false);
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

        PlayAnimation();
    }

    private void PlayAnimation()
    {
        sequence?.Kill();

        float vx = Random.Range(-0.4f, 0.4f);
        Vector3 end_pos = transform.position + new Vector3(vx * lifetime, move_speed_y * lifetime * DriftHeightFactor, 0f);

        transform.localScale = Vector3.one * PunchStartScale;

        sequence = DOTween.Sequence();

        // Float upwards & drift
        sequence.Append(transform.DOMove(end_pos, lifetime).SetEase(Ease.OutQuad));

        // Punch scale: pop up fast, then settle
        sequence.Insert(0f, transform.DOScale(PunchPeakScale, lifetime * PunchUpFraction).SetEase(Ease.Linear));
        sequence.Insert(lifetime * PunchUpFraction, transform.DOScale(1f, lifetime * (1f - PunchUpFraction)).SetEase(Ease.Linear));

        // Fade alpha in 2nd half
        if (text_mesh != null)
        {
            sequence.Insert(lifetime * FadeStartFraction,
                DOTween.To(() => text_mesh.alpha, a => text_mesh.alpha = a, 0f, lifetime * (1f - FadeStartFraction)).SetEase(Ease.Linear));
        }

        sequence.OnComplete(ReleaseToPool);
    }

    private void ReleaseToPool()
    {
        transform.localScale = Vector3.one;
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        sequence?.Kill();
        sequence = null;
    }

    private void OnDestroy()
    {
        sequence?.Kill();
        sequence = null;
    }
}
