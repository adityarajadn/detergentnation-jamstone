using UnityEngine;

public class WeaponBubbleGun : RangedWeapon
{
    protected override void SpawnProjectile(Vector3 pos, Vector2 dir, int dmg)
    {
        base.SpawnProjectile(pos, dir, dmg);
    }
}
