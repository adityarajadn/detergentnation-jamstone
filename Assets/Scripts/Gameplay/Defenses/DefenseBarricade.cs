using UnityEngine;

public class DefenseBarricade : DefenseObject
{
    [SerializeField] private int repair_heal = 30;

    protected override bool CanInteract()
    {
        return health != null && health.health_current < health.health_max;
    }

    protected override string GetInteractionPrompt(string keyName)
    {
        return "[" + keyName + "] REPAIR FENCE";
    }

    protected override void OnInteract()
    {
        if (health != null)
        {
            health.Heal(repair_heal);
        }
    }

    protected override void OnHealthChanged(int cur, int max)
    {
        if (cur < max && cur > 0)
        {
            AudioManager.PlaySFX("fence_damage");
        }
    }

    protected override void PlayBreakAudio()
    {
        AudioManager.PlaySFX("fence_break");
    }
}
