using UnityEngine;

public class WeaponCurseSwordImelda : MeleeWeapon
{
    private const int LethalDamage = 99999;

    protected override void ApplyHitToEnemy(Collider2D enemy_col, int damage)
    {
        Health hp = enemy_col.GetComponent<Health>();
        if (hp != null && !hp.is_dead)
        {
            hp.TakeDamage(LethalDamage);
        }
    }
}
