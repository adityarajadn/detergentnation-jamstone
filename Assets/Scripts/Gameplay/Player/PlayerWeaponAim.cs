using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponAim : MonoBehaviour
{
    public Transform weapon_mount;
    [SerializeField] private Transform visuals;
    [SerializeField] private float mount_offset_x = 0.2f;
    [SerializeField] private float mount_offset_y = -0.05f;
    [SerializeField] private float aim_deadzone = 0.25f;

    private Camera main_camera;
    private PlayerController player_controller;
    private bool last_aim_left = false;

    private void Awake()
    {
        main_camera = Camera.main;
        player_controller = GetComponent<PlayerController>();
        if (visuals == null)
        {
            visuals = transform.Find("Visuals");
        }
        EnsureWeaponMount();
    }

    public void EnsureWeaponMount()
    {
        if (weapon_mount == transform)
        {
            weapon_mount = null;
        }

        if (weapon_mount == null)
        {
            Transform found = transform.Find("WeaponMount");
            if (found == null) found = transform.Find("Visuals/GunArm");
            if (found == null)
            {
                GameObject mount_go = new GameObject("WeaponMount");
                mount_go.transform.SetParent(transform, false);
                mount_go.transform.localPosition = new Vector3(mount_offset_x, mount_offset_y, 0f);
                found = mount_go.transform;
            }
            weapon_mount = found;
        }
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

        EnsureWeaponMount();
        if (weapon_mount == null || weapon_mount == transform) return;
        if (Mouse.current == null) return;
        if (main_camera == null) main_camera = Camera.main;
        if (main_camera == null) return;

        Vector3 mouse_screen = Mouse.current.position.ReadValue();
        Vector3 mouse_world = main_camera.ScreenToWorldPoint(mouse_screen);

        // Determine aim direction relative to player center with deadzone hysteresis to prevent jitter
        float delta_x_from_player = mouse_world.x - transform.position.x;
        if (Mathf.Abs(delta_x_from_player) > aim_deadzone)
        {
            last_aim_left = delta_x_from_player < 0f;
        }
        bool aim_left = last_aim_left;

        float offset_x = Mathf.Abs(mount_offset_x);
        weapon_mount.localPosition = new Vector3(aim_left ? -offset_x : offset_x, mount_offset_y, 0f);

        Vector2 aim_delta = mouse_world - weapon_mount.position;
        if (aim_delta.sqrMagnitude > 0.0001f)
        {
            float aim_angle = Mathf.Atan2(aim_delta.y, aim_delta.x) * Mathf.Rad2Deg;
            weapon_mount.rotation = Quaternion.Euler(0f, 0f, aim_angle);
        }

        weapon_mount.localScale = new Vector3(1f, aim_left ? -1f : 1f, 1f);

        // Keep player body completely upright (0 rotation)
        transform.rotation = Quaternion.identity;

        // Flip visuals horizontally: prioritize movement input over aim direction
        if (visuals != null)
        {
            float vx = Mathf.Abs(visuals.localScale.x);
            bool face_left;
            if (player_controller != null && Mathf.Abs(player_controller.MoveInput) > 0.05f)
            {
                face_left = player_controller.MoveInput < 0f;
            }
            else
            {
                face_left = aim_left;
            }
            visuals.localScale = new Vector3(face_left ? -vx : vx, visuals.localScale.y, visuals.localScale.z);
        }
    }
}
