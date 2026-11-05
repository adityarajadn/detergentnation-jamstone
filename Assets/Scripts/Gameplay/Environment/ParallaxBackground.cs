using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform transform;
        [Range(0f, 1f)] public float parallax_x = 0.5f;
        [Range(0f, 1f)] public float parallax_y = 0.1f;
        public bool infinite_horizontal = false;
        [HideInInspector] public float texture_width;
        [HideInInspector] public Vector3 start_pos;
    }

    [SerializeField] private Transform camera_transform;
    [SerializeField] private ParallaxLayer[] layers;

    private Vector3 last_camera_pos;

    private void Awake()
    {
        if (camera_transform == null && Camera.main != null)
        {
            camera_transform = Camera.main.transform;
        }
    }

    private void Start()
    {
        if (camera_transform == null && Camera.main != null)
        {
            camera_transform = Camera.main.transform;
        }

        if (camera_transform != null)
        {
            last_camera_pos = camera_transform.position;
        }

        if (layers != null)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].transform == null) continue;
                layers[i].start_pos = layers[i].transform.position;

                SpriteRenderer sr = layers[i].transform.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    layers[i].texture_width = sr.bounds.size.x;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (camera_transform == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                camera_transform = cam.transform;
                last_camera_pos = camera_transform.position;
            }
            return;
        }

        Vector3 delta = camera_transform.position - last_camera_pos;

        if (layers != null)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                ParallaxLayer layer = layers[i];
                if (layer.transform == null) continue;

                Vector3 target_pos = layer.transform.position;
                target_pos.x += delta.x * layer.parallax_x;
                target_pos.y += delta.y * layer.parallax_y;
                layer.transform.position = target_pos;

                // ponytail: wrap horizontally when texture_width > 0 and infinite_horizontal true
                if (layer.infinite_horizontal && layer.texture_width > 0f)
                {
                    float dist_from_cam = camera_transform.position.x - layer.transform.position.x;
                    if (Mathf.Abs(dist_from_cam) >= layer.texture_width)
                    {
                        float offset_x = (dist_from_cam > 0 ? 1 : -1) * layer.texture_width;
                        layer.transform.position = new Vector3(layer.transform.position.x + offset_x, layer.transform.position.y, layer.transform.position.z);
                    }
                }
            }
        }

        last_camera_pos = camera_transform.position;
    }
}
