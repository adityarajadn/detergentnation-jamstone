using System;
using UnityEngine;

public class Minimarket : MonoBehaviour
{
    public int healthMax = 100;
    public int healthCurrent = 100;
    public bool isDestroyed = false;

    public bool playerInRestockZone = false;
    public bool playerInRepairZone = false;

    public event Action<int, int> OnHealthChanged;

    public void TakeDamage(int dmg)
    {
        if (isDestroyed) return;
        healthCurrent = Mathf.Max(0, healthCurrent - dmg);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
        AudioManager.Instance?.PlaySfx("hit");

        if (healthCurrent <= 0)
        {
            isDestroyed = true;
            EventBus.RaiseLevelFailed("RnA Mart ice cream was looted!");
        }
    }

    public void Repair(int amount)
    {
        if (isDestroyed || healthCurrent >= healthMax) return;
        healthCurrent = Mathf.Min(healthMax, healthCurrent + amount);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
        AudioManager.Instance?.PlaySfx("repair");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.transform.position.x < transform.position.x) playerInRestockZone = true;
            else playerInRepairZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRestockZone = false;
            playerInRepairZone = false;
        }
    }
}
