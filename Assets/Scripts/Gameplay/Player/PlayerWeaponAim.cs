using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponAim : MonoBehaviour
{
    [Header("Mount & Aiming")]
    public Transform hand_mount;
    [SerializeField] private Transform visuals;
    [SerializeField] private float aim_deadzone = 0.25f;


    private Camera main_camera;
    private PlayerController player_controller;
    private bool last_aim_left = false;

    private void Awake()
    {
        main_camera = Camera.main;
        player_controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        AimWeapon();
    }

    public void AimWeapon()
    {
        if (Time.timeScale == 0f) return;
        if (LevelManager.instance != null)
        {
            GamePhase phase = LevelManager.instance.current_phase;
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat) return;
        }

        if (hand_mount == null || hand_mount == transform) return;
        if (Mouse.current == null) return;
        if (main_camera == null) main_camera = Camera.main;
        if (main_camera == null) return;

        Vector3 mouse_screen = Mouse.current.position.ReadValue();
        Vector3 mouse_world = main_camera.ScreenToWorldPoint(mouse_screen);

        // Determine if mouse is left or right relative to player
        float delta_x_from_player = mouse_world.x - transform.position.x;
        bool aim_left = delta_x_from_player < -aim_deadzone;
        bool aim_right = delta_x_from_player > aim_deadzone;

        float vx = Mathf.Abs(visuals != null ? visuals.localScale.x : 1f);
        bool face_left;
        if (player_controller != null && Mathf.Abs(player_controller.MoveInput) > 0.05f)
        {
            face_left = player_controller.MoveInput < 0f;
        }
        else
        {
            face_left = aim_left || (last_aim_left && !aim_right);
        }
        last_aim_left = face_left;

        // Flip visuals horizontally based on movement input or aim direction
        if (visuals != null)
        {
            visuals.localScale = new Vector3(face_left ? -vx : vx, visuals.localScale.y, visuals.localScale.z);
        }

        // Aim rotation on hand_mount (child of Visuals)
        Vector2 aim_delta = mouse_world - hand_mount.position;
        if (aim_delta.sqrMagnitude > 0.0001f)
        {
            float aim_angle = Mathf.Atan2(aim_delta.y, aim_delta.x) * Mathf.Rad2Deg;
            if (face_left)
            {
                aim_angle = 180f - aim_angle;
            }
            hand_mount.localRotation = Quaternion.Euler(0f, 0f, aim_angle);
        }

        // Keep player body upright
        transform.rotation = Quaternion.identity;
    }
}
