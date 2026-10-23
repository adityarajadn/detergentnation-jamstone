using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int healthMax = 100;
    public int healthCurrent = 100;
    public int armorBonus = 0;
    public int shieldCharges = 0;

    public event Action<int, int> OnHealthChanged;

    public void SetupArmor(string armorId)
    {
        armorBonus = armorId switch
        {
            "worker_hat" => 1,
            "number_one" => 2,
            "octosoap" => 3,
            _ => 0
        };
    }

    public void TakeDamage(int amount)
    {
        if (shieldCharges > 0)
        {
            shieldCharges--;
            AudioManager.Instance?.PlaySfx("hit");
            return;
        }

        int actualDmg = Mathf.Max(1, amount - armorBonus);
        healthCurrent = Mathf.Max(0, healthCurrent - actualDmg);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
        EventBus.RaisePlayerDamaged();
        AudioManager.Instance?.PlaySfx("hit");

        if (healthCurrent <= 0)
        {
            EventBus.RaisePlayerDied();
        }
    }

    public void Heal(int amount)
    {
        healthCurrent = Mathf.Min(healthMax, healthCurrent + amount);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
    }
}
