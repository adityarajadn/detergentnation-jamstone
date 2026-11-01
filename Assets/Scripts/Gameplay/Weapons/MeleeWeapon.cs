using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee Setup")]
    public Transform melee_point;

    protected override void Awake()
    {
        base.Awake();
        if (melee_point == null)
        {
            Transform mp = transform.Find("MeleePoint");
            melee_point = mp != null ? mp : transform;
        }
    }

    public virtual void PerformStrike(Vector3 origin, float range, int damage, LayerMask enemy_layer)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, enemy_layer);
        for (int i = 0; i < hits.Length; i++)
        {
            ApplyHitToEnemy(hits[i], damage);
        }
    }

    protected virtual void ApplyHitToEnemy(Collider2D enemy_col, int damage)
    {
        // Ignore sensor/trigger colliders (e.g. EnemyAggroDetector)
        if (enemy_col.isTrigger || enemy_col.GetComponent<EnemyAggroDetector>() != null) return;

        // Prevent damage to friendly entities: Minimarket, Defense, Player
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
            hp.TakeDamage(damage);
            string hitSound = "hit_imelda";
            if (weapon_data != null)
            {
                string wn = weapon_data.weapon_name.ToLower();
                if (wn.Contains("hammer")) hitSound = "hit_hammer";
                else if (wn.Contains("stick")) hitSound = "hit_stick";
            }
            AudioManager.PlaySFX(hitSound);
        }
    }

    public override string GetAmmoDisplayText(PlayerAmmo ammo)
    {
        return weapon_data != null ? weapon_data.weapon_name : "MELEE";
    }
}
