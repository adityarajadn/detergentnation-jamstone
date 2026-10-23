using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCameraTarget : MonoBehaviour
{
    public Transform player;
    public Camera cam;

    [Header("Mouse Peek Settings")]
    [Range(0f, 1f)] public float horizontalWeight = 0.3f;
    [Range(0f, 1f)] public float verticalWeight = 0.25f;
    public float maxOffsetX = 2.5f;
    public float maxOffsetY = 1.6f;
    public float baseOffsetY = 0.6f;

    private void Start()
    {
        if (player == null)
        {
            var p = FindAnyObjectByType<Player>();
            if (p != null) player = p.transform;
        }
        if (cam == null) cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (player == null) return;
        if (cam == null) cam = Camera.main;

        Vector3 mScreen = Input.mousePosition;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            var mPos = Mouse.current.position.ReadValue();
            mScreen = new Vector3(mPos.x, mPos.y, 0f);
        }
#endif

        float offX = 0f;
        float offY = 0f;

        if (cam != null)
        {
            float zDistance = Mathf.Abs(cam.transform.position.z - player.position.z);
            if (zDistance <= 0.01f) zDistance = 10f;
            Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mScreen.x, mScreen.y, zDistance));
            Vector2 diff = (Vector2)mouseWorld - (Vector2)player.position;

            // Kursor kanan / kiri
            offX = Mathf.Clamp(diff.x * horizontalWeight, -maxOffsetX, maxOffsetX);

            // Kursor atas saja, arah bawah diabaikan (kamera tidak turun ke bawah)
            if (diff.y > 0f)
            {
                offY = Mathf.Clamp(diff.y * verticalWeight, 0f, maxOffsetY);
            }
        }

        transform.position = new Vector3(
            player.position.x + offX,
            player.position.y + offY + baseOffsetY,
            0f
        );
    }
}
