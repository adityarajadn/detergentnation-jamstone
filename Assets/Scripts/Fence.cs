using UnityEngine;

public class Fence : MonoBehaviour
{
    public int healthMax = 100;
    public int healthCurrent = 100;
    private bool playerInRange = false;

    public void TakeDamage(int dmg)
    {
        healthCurrent = Mathf.Max(0, healthCurrent - dmg);
        AudioManager.Instance?.PlaySfx("hit");
        if (healthCurrent <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Repair(int amount)
    {
        healthCurrent = Mathf.Min(healthMax, healthCurrent + amount);
        AudioManager.Instance?.PlaySfx("repair");
    }

    private void Update()
    {
        bool interact = Input.GetKeyDown(KeybindManager.Interact) || Input.GetKeyDown(KeyCode.F);
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame) interact = true;
#endif

        if (playerInRange && interact && healthCurrent < healthMax)
        {
            var p = FindAnyObjectByType<Player>();
            if (p != null && p.interaction != null && p.interaction.repairPoints > 0)
            {
                int needed = healthMax - healthCurrent;
                int use = Mathf.Min(needed, p.interaction.repairPoints);
                p.interaction.repairPoints -= use;
                Repair(use);
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
