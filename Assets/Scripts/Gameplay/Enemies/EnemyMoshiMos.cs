using UnityEngine;

public class EnemyMoshiMos : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        if (health != null)
        {
            health.SetShieldCharges(2);
        }
    }
}
