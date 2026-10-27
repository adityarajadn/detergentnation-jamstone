using UnityEngine;

public class GlobalUIManager : MonoBehaviour
{
    public static GlobalUIManager instance { get; private set; }

    [Header("Global UI Modules")]
    [SerializeField] private NotificationToast toast_controller;

    private const string ResourcePath = "UI/GlobalUI";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (toast_controller == null)
        {
            toast_controller = GetComponentInChildren<NotificationToast>(true);
        }
    }

    public static GlobalUIManager EnsureInstance()
    {
        if (instance == null)
        {
            instance = FindAnyObjectByType<GlobalUIManager>();
            if (instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>(ResourcePath);
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.name = "GlobalUI";
                    instance = obj.GetComponent<GlobalUIManager>();
                }
                else
                {
                    Debug.LogWarning("[GlobalUIManager] Prefab not found at Resources/" + ResourcePath);
                }
            }
        }
        return instance;
    }

    public static void ShowToast(string message, ToastType type = ToastType.Info)
    {
        GlobalUIManager mgr = EnsureInstance();
        if (mgr != null && mgr.toast_controller != null)
        {
            mgr.toast_controller.TriggerToast(message, type);
        }
        else
        {
            Debug.Log("[GlobalUIManager Toast] " + type + ": " + message);
        }
    }
}
