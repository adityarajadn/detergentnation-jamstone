using UnityEngine;

public class SoapCannon : MonoBehaviour
{
    public int ammo = 10;
    public float range = 8.0f;
    public float fireCooldown = 1.0f;
    public Transform barrel;
    public Transform muzzle;
    public GameObject soapBulletPrefab;
    public LayerMask enemyLayer;

    private float timer = 0f;
    private bool playerInRange = false;

    private void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;

        bool interact = Input.GetKeyDown(KeyCode.E);
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame) interact = true;
#endif

        if (playerInRange && interact && timer <= 0f && ammo > 0)
        {
            FireSpread();
        }
    }

    private void FireSpread()
    {
        // Find closest enemy
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        Vector2 aimDir = Vector2.right;
        if (hits.Length > 0)
        {
            float closestDist = float.MaxValue;
            foreach (var col in hits)
            {
                float d = Vector2.Distance(transform.position, col.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    aimDir = (col.transform.position - transform.position).normalized;
                }
            }
        }

        if (barrel != null)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            barrel.rotation = Quaternion.Euler(0, 0, angle);
        }

        ammo--;
        timer = fireCooldown;
        AudioManager.Instance?.PlaySfx("shoot");

        Vector3 muzPos = muzzle != null ? muzzle.position : transform.position;
        if (soapBulletPrefab != null)
        {
            for (int i = 0; i < 4; i++)
            {
                float spread = (i - 1.5f) * 10f;
                Vector2 dir = Quaternion.Euler(0, 0, spread) * aimDir;
                GameObject b = Instantiate(soapBulletPrefab, muzPos, Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
                var bullet = b.GetComponent<SoapBullet>();
                if (bullet != null)
                {
                    bullet.direction = dir;
                    bullet.damage = 30;
                    bullet.speed = 12f;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player")) playerInRange = false;
    }
}
