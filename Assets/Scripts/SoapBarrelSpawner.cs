using UnityEngine;

public class SoapBarrelSpawner : MonoBehaviour
{
    public int ammo = 5;
    public GameObject soapBarrelPrefab;
    public Transform spawnPoint;
    public Vector2 rollDirection = Vector2.right;

    private bool playerInRange = false;

    private void Update()
    {
        bool interact = Input.GetKeyDown(KeybindManager.Interact) || Input.GetKeyDown(KeyCode.F);
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame) interact = true;
#endif

        if (playerInRange && interact && ammo > 0)
        {
            SpawnBarrel();
        }
    }

    private void SpawnBarrel()
    {
        ammo--;
        AudioManager.Instance?.PlaySfx("shoot");
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        if (soapBarrelPrefab != null)
        {
            GameObject barrel = Instantiate(soapBarrelPrefab, pos, Quaternion.identity);
            var sb = barrel.GetComponent<SoapBarrel>();
            if (sb != null) sb.rollDirection = rollDirection;
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
