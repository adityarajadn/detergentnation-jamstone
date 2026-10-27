using UnityEngine;

public class EnemyMoshiMos : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        if (health != null)
        {
            health.shield_charges = 2;
        }
    }
}
