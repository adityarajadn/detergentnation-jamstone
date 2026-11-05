using System.Collections.Generic;
using UnityEngine;

public class BatSwarmEntity : MonoBehaviour
{
    [SerializeField] private float duration = 30f;
    [SerializeField] private float move_speed = 7f;
    [SerializeField] private float stun_refresh_interval = 0.5f;

    private float timer = 0f;
    private float stun_timer = 0f;
    private EnemyBase current_target;
    private SpriteRenderer sprite_renderer;

    private void Awake()
    {
        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FindNextTarget();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Despawn();
            return;
        }

        // Validate current target
        if (current_target == null || current_target.IsDead || !current_target.gameObject.activeInHierarchy)
        {
            current_target = null;
            FindNextTarget();
        }

        if (current_target != null)
        {
            // Move towards current target
            Vector3 target_pos = current_target.transform.position + Vector3.up * 0.3f;
            transform.position = Vector3.MoveTowards(transform.position, target_pos, move_speed * Time.deltaTime);

            if (sprite_renderer != null)
            {
                sprite_renderer.flipX = target_pos.x < transform.position.x;
            }

            // Continuously apply stun while on the enemy
            float dist = Vector2.Distance(transform.position, target_pos);
            if (dist <= 0.6f)
            {
                stun_timer += Time.deltaTime;
                if (stun_timer >= stun_refresh_interval)
                {
                    stun_timer = 0f;
                    current_target.ApplyStun(1.5f);
                }
            }
        }
    }

    private void FindNextTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(GameConstants.Tags.Enemy);
        List<EnemyBase> alive_enemies = new List<EnemyBase>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;
            EnemyBase eb = enemies[i].GetComponent<EnemyBase>();
            if (eb != null && !eb.IsDead && eb.gameObject.activeInHierarchy)
            {
                alive_enemies.Add(eb);
            }
        }

        if (alive_enemies.Count > 0)
        {
            // Pick random alive enemy
            int index = Random.Range(0, alive_enemies.Count);
            current_target = alive_enemies[index];
            stun_timer = stun_refresh_interval; // stun immediately upon arrival
        }
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
