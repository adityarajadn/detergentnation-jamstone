using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private PlayerCombat combat_player;
    private readonly HashSet<Collider2D> hit_targets = new HashSet<Collider2D>();

    private void Awake()
    {
        combat_player = GetComponentInParent<PlayerCombat>();
    }

    private void OnEnable()
    {
        hit_targets.Clear();
    }

    private void OnDisable()
    {
        hit_targets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (hit_targets.Contains(other)) return;

        // Ignore sensor/trigger colliders (e.g. EnemyAggroDetector)
        if (other.isTrigger || other.GetComponent<EnemyAggroDetector>() != null) return;

        // Prevent damage to friendly entities: Minimarket, Defense, Player
        if (other.CompareTag(GameConstants.Tags.Minimarket)
            || other.CompareTag(GameConstants.Tags.Defense)
            || other.CompareTag(GameConstants.Tags.Player)
            || other.GetComponentInParent<Minimarket>() != null)
        {
            return;
        }

        // Only target enemies (layer or tag or EnemyBase component)
        bool isEnemy = other.gameObject.layer == LayerMask.NameToLayer(GameConstants.Layers.Enemy)
            || other.CompareTag(GameConstants.Tags.Enemy)
            || other.GetComponentInParent<EnemyBase>() != null;

        if (!isEnemy) return;

        Health hp = other.GetComponent<Health>();
        if (hp == null) hp = other.GetComponentInParent<Health>();

        if (hp != null && !hp.is_dead)
        {
            // Do not damage self/player or defenses
            if (hp.gameObject == transform.root.gameObject) return;

            hit_targets.Add(other);

            int dmg = 25;
            string hitSound = GameConstants.AudioKeys.HitImelda;

            if (combat_player != null && combat_player.melee_weapon != null)
            {
                dmg = combat_player.melee_weapon.damage;
                string key = combat_player.melee_weapon.hit_sound_key;
                if (!string.IsNullOrEmpty(key)) hitSound = key;
            }

            hp.TakeDamage(dmg);
            AudioManager.PlaySFX(hitSound);
        }
    }
}
