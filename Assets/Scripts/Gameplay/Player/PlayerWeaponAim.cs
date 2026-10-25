using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponAim : MonoBehaviour
{
    public Transform weapon_mount;
    [SerializeField] private Transform visuals;
    [SerializeField] private float mount_offset_x = 0.2f;
    [SerializeField] private float mount_offset_y = -0.05f;

    private Camera main_camera;

    private void Awake()
    {
        main_camera = Camera.main;
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
        EnsureWeaponMount();
        if (weapon_mount == null || weapon_mount == transform) return;
        if (Mouse.current == null) return;
        if (main_camera == null) main_camera = Camera.main;
        if (main_camera == null) return;

        Vector3 mouse_screen = Mouse.current.position.ReadValue();
        Vector3 mouse_world = main_camera.ScreenToWorldPoint(mouse_screen);
        Vector2 aim_delta = mouse_world - weapon_mount.position;
        float aim_angle = Mathf.Atan2(aim_delta.y, aim_delta.x) * Mathf.Rad2Deg;

        // ONLY weapon mount rotates
        weapon_mount.rotation = Quaternion.Euler(0f, 0f, aim_angle);

        bool aim_left = aim_delta.x < 0f;
        weapon_mount.localScale = new Vector3(1f, aim_left ? -1f : 1f, 1f);

        float offset_x = Mathf.Abs(mount_offset_x);
        weapon_mount.localPosition = new Vector3(aim_left ? -offset_x : offset_x, mount_offset_y, 0f);

        // Keep player body completely upright (0 rotation)
        transform.rotation = Quaternion.identity;

        // Flip visuals horizontally based on aim direction
        if (visuals != null)
        {
            float vx = Mathf.Abs(visuals.localScale.x);
            visuals.localScale = new Vector3(aim_left ? -vx : vx, visuals.localScale.y, visuals.localScale.z);
        }
    }
}
