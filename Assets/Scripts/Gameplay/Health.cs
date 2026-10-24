using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int health_max = 100;
    public int health_current;
    public int shield_charges = 0;
    public bool is_dead = false;

    public UnityEvent<int, int> on_health_changed;
    public UnityEvent on_death;

    private void Awake()
    {
        health_current = health_max;
    }

    public void TakeDamage(int amount)
    {
        if (is_dead || amount <= 0) return;

        if (shield_charges > 0)
        {
            shield_charges--;
            return;
        }

        health_current -= amount;
        if (health_current < 0) health_current = 0;

        if (on_health_changed != null)
        {
            on_health_changed.Invoke(health_current, health_max);
        }

        if (health_current <= 0)
        {
            is_dead = true;
            if (on_death != null)
            {
                on_death.Invoke();
            }
        }
    }

    public void Heal(int amount)
    {
        if (is_dead || amount <= 0) return;

        health_current += amount;
        if (health_current > health_max) health_current = health_max;

        if (on_health_changed != null)
        {
            on_health_changed.Invoke(health_current, health_max);
        }
    }

    public void SetMaxHealth(int new_max, bool heal_to_full = true)
    {
        health_max = new_max;
        if (heal_to_full)
        {
            health_current = health_max;
        }
        else if (health_current > health_max)
        {
            health_current = health_max;
        }

        if (on_health_changed != null)
        {
            on_health_changed.Invoke(health_current, health_max);
        }
    }
}
