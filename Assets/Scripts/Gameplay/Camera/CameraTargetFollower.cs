using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float max_offset_x = 2.0f;
    [SerializeField] private float max_offset_y = 1.2f;
    [SerializeField] private float smooth_speed = 12f;

    private Camera cached_cam;

    private void Awake()
    {
        cached_cam = Camera.main;
    }

    private void Start()
    {
        if (target == null && PlayerController.Instance != null)
        {
            target = PlayerController.Instance.transform;
        }

        if (target != null)
        {
            transform.position = target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            return;
        }

        Vector3 mouse_offset = Vector3.zero;
        Mouse mouse = Mouse.current;
        if (mouse != null && Screen.width > 0 && Screen.height > 0)
        {
            Vector2 mouse_pos = mouse.position.ReadValue();
            float norm_x = Mathf.Clamp((mouse_pos.x / Screen.width - 0.5f) * 2f, -1f, 1f);
            float norm_y = Mathf.Clamp((mouse_pos.y / Screen.height - 0.5f) * 2f, -1f, 1f);

            mouse_offset = new Vector3(norm_x * max_offset_x, norm_y * max_offset_y, 0f);
        }

        Vector3 desired_pos = target.position + mouse_offset;
        transform.position = Vector3.Lerp(transform.position, desired_pos, smooth_speed * Time.deltaTime);
    }
}
