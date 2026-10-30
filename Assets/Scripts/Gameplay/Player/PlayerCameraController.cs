using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    public Transform target;
    public float smooth_time = 0.1f;
    public float max_look_ahead_x = 2.0f;
    public float max_look_ahead_y = 1.2f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    public float min_x = -13f;
    public float max_x = 13f;
    public float min_y = -1f;
    public float max_y = 7f;

    private Vector3 current_velocity;
    private float shake_timer = 0f;
    private float shake_intensity = 0f;

    public static PlayerCameraController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Shake(float duration = 0.2f, float intensity = 0.3f)
    {
        shake_timer = duration;
        shake_intensity = intensity;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
            if (player != null)
            {
                target = player.transform;
            }
            return;
        }

        Vector3 mouse_offset = Vector3.zero;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mouse_screen = mouse.position.ReadValue();
            float norm_x = (mouse_screen.x / Screen.width - 0.5f) * 2f;
            float norm_y = (mouse_screen.y / Screen.height - 0.5f) * 2f;

            norm_x = Mathf.Clamp(norm_x, -1f, 1f);
            norm_y = Mathf.Clamp(norm_y, -1f, 1f);

            mouse_offset = new Vector3(norm_x * max_look_ahead_x, norm_y * max_look_ahead_y, 0f);
        }

        Vector3 desired_pos = target.position + offset + mouse_offset;
        desired_pos.x = Mathf.Clamp(desired_pos.x, min_x, max_x);
        desired_pos.y = Mathf.Clamp(desired_pos.y, min_y, max_y);
        desired_pos.z = offset.z;

        Vector3 target_pos = Vector3.SmoothDamp(transform.position, desired_pos, ref current_velocity, smooth_time);

        if (shake_timer > 0f)
        {
            shake_timer -= Time.deltaTime;
            Vector2 offset_shake = Random.insideUnitCircle * shake_intensity;
            target_pos.x += offset_shake.x;
            target_pos.y += offset_shake.y;
        }

        transform.position = target_pos;
    }
}
