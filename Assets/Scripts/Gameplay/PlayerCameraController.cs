using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    public Transform target;
    public float smooth_time = 0.25f;
    public float max_look_ahead_x = 3.5f;
    public float max_look_ahead_y = 2.0f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    public float min_x = -13f;
    public float max_x = 13f;
    public float min_y = -1f;
    public float max_y = 7f;

    private Vector3 current_velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
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

        transform.position = Vector3.SmoothDamp(transform.position, desired_pos, ref current_velocity, smooth_time);
    }
}
