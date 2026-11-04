using UnityEngine;

public class WeaponCurseSwordImelda : MeleeWeapon
{
    private const int LethalDamage = 99999;

    protected override void ApplyHitToEnemy(Collider2D enemy_col, int damage)
    {
        // Ignore sensor/trigger colliders (e.g. EnemyAggroDetector)
        if (enemy_col.isTrigger || enemy_col.GetComponent<EnemyAggroDetector>() != null) return;

        if (enemy_col.CompareTag(GameConstants.Tags.Minimarket)
            || enemy_col.CompareTag(GameConstants.Tags.Defense)
            || enemy_col.CompareTag(GameConstants.Tags.Player)
            || enemy_col.GetComponentInParent<Minimarket>() != null)
        {
            return;
        }

        bool isEnemy = enemy_col.gameObject.layer == LayerMask.NameToLayer(GameConstants.Layers.Enemy)
            || enemy_col.CompareTag(GameConstants.Tags.Enemy)
            || enemy_col.GetComponentInParent<EnemyBase>() != null;

        if (!isEnemy) return;

        Health hp = enemy_col.GetComponent<Health>();
        if (hp == null) hp = enemy_col.GetComponentInParent<Health>();

        if (hp != null && !hp.is_dead)
        {
            hp.TakeDamage(LethalDamage);
            AudioManager.PlaySFX(GameConstants.AudioKeys.HitImelda);
        }
    }
}
