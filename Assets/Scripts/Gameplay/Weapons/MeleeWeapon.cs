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
        Health hp = enemy_col.GetComponent<Health>();
        if (hp != null && !hp.is_dead)
        {
            hp.TakeDamage(damage);
            string hitSound = "hit_curse_sword";
            if (weapon_data != null)
            {
                string wn = weapon_data.weapon_name.ToLower();
                if (wn.Contains("hammer")) hitSound = "hit_soap_hammer";
                else if (wn.Contains("stick")) hitSound = "hit_bubble_stick";
            }
            AudioManager.PlaySFX(hitSound);
        }
    }
}
