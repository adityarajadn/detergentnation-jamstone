using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponAim : MonoBehaviour
{
    public Transform weapon_mount;
    private Camera main_camera;

    private void Awake()
    {
        main_camera = Camera.main;
        if (weapon_mount == null)
        {
            Transform found = transform.Find("Visuals/GunArm");
            if (found != null) weapon_mount = found;
        }
    }

    private void Update()
    {
        AimWeapon();
    }

    public void AimWeapon()
    {
        if (weapon_mount == null || Mouse.current == null) return;
        if (main_camera == null) main_camera = Camera.main;
        if (main_camera == null) return;

        Vector3 mouse_screen = Mouse.current.position.ReadValue();
        Vector3 mouse_world = main_camera.ScreenToWorldPoint(mouse_screen);
        Vector2 aim_delta = mouse_world - weapon_mount.position;
        float aim_angle = Mathf.Atan2(aim_delta.y, aim_delta.x) * Mathf.Rad2Deg;

        weapon_mount.rotation = Quaternion.Euler(0f, 0f, aim_angle);
        weapon_mount.localScale = new Vector3(1f, aim_delta.x < 0f ? -1f : 1f, 1f);
    }
}
