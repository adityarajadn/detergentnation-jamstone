using System.Collections;
using TMPro;
using UnityEngine;

public class DefenseObject : MonoBehaviour
{
    public DefenseData defense_data;
    public Health health;
    public int ammo_current;

    [SerializeField] private GameObject projectile_prefab;
    [SerializeField] private float roll_speed = 9f;
    [SerializeField] private TMP_Text prompt_text;

    private bool is_rolling = false;
    private float roll_direction = 1f;
    private float next_cannon_fire_time = 0f;
    private GameObject visuals_container;

    private void Awake()
    {
        health = GetComponent<Health>();
        Transform vis = transform.Find("Visuals");
        if (vis != null) visuals_container = vis.gameObject;
        if (prompt_text == null) prompt_text = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        if (defense_data != null)
        {
            if (defense_data.defense_type == DefenseType.HealthBased)
            {
                if (health != null) health.SetMaxHealth(defense_data.max_health, true);
            }
            ammo_current = defense_data.max_ammo;
        }

        if (health != null)
        {
            health.on_death.AddListener(OnDestroyed);
        }

        if (prompt_text != null) prompt_text.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (is_rolling)
        {
            transform.Translate(Vector3.right * (roll_direction * roll_speed * Time.deltaTime));
            if (visuals_container != null)
            {
                visuals_container.transform.Rotate(0f, 0f, -roll_direction * 360f * Time.deltaTime);
            }
            return;
        }

        HandlePlayerProximityInteraction();
    }

    private void HandlePlayerProximityInteraction()
    {
        if (LevelManager.instance != null && LevelManager.instance.current_phase == GamePhase.Planning)
        {
            if (prompt_text != null) prompt_text.gameObject.SetActive(false);
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            if (prompt_text != null) prompt_text.gameObject.SetActive(false);
            return;
        }

        float dist = Vector2.Distance(player.transform.position, transform.position);
        bool is_near = dist <= 2.2f;

        if (defense_data == null) return;

        if (defense_data.defense_name.Contains("Cannon"))
        {
            if (is_near && ammo_current > 0)
            {
                if (prompt_text != null)
                {
                    prompt_text.gameObject.SetActive(true);
                    prompt_text.text = "[E] FIRE CANNON (" + ammo_current + "/" + defense_data.max_ammo + ")";
                }

                UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && (kb.eKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame))
                {
                    if (Time.time >= next_cannon_fire_time)
                    {
                        FireCannon();
                    }
                }
            }
            else
            {
                if (prompt_text != null) prompt_text.gameObject.SetActive(false);
            }
        }
        else if (defense_data.defense_name.Contains("Barrel"))
        {
            if (is_near && !is_rolling)
            {
                if (prompt_text != null)
                {
                    prompt_text.gameObject.SetActive(true);
                    prompt_text.text = "[E] ROLL BARREL";
                }

                UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && (kb.eKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame))
                {
                    ReleaseBarrel();
                }
            }
            else
            {
                if (prompt_text != null) prompt_text.gameObject.SetActive(false);
            }
        }
        else if (defense_data.defense_type == DefenseType.HealthBased)
        {
            if (is_near && health != null && health.health_current < health.health_max)
            {
                if (prompt_text != null)
                {
                    prompt_text.gameObject.SetActive(true);
                    prompt_text.text = "[E] REPAIR FENCE";
                }

                UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && (kb.eKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame))
                {
                    health.Heal(30);
                }
            }
            else
            {
                if (prompt_text != null) prompt_text.gameObject.SetActive(false);
            }
        }
    }

    public void FireCannon()
    {
        if (ammo_current <= 0) return;

        ammo_current--;
        next_cannon_fire_time = Time.time + 0.5f;

        // Determine target direction (outwards towards enemy flank or nearest enemy)
        Vector2 fire_dir = transform.position.x < 0 ? Vector2.left : Vector2.right;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closest_dist = 12f;
        for (int i = 0; i < enemies.Length; i++)
        {
            float d = Vector2.Distance(transform.position, enemies[i].transform.position);
            if (d < closest_dist)
            {
                closest_dist = d;
                fire_dir = (enemies[i].transform.position - transform.position).normalized;
            }
        }

        float angle = Mathf.Atan2(fire_dir.y, fire_dir.x) * Mathf.Rad2Deg;
        GameObject proj_prefab = projectile_prefab;
        if (proj_prefab == null) proj_prefab = Resources.Load<GameObject>("Prefabs/Projectile");
        if (proj_prefab == null) proj_prefab = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Projectile.prefab", typeof(GameObject));

        if (proj_prefab != null)
        {
            GameObject proj_obj = Instantiate(proj_prefab, transform.position + (Vector3)fire_dir * 0.8f + Vector3.up * 0.2f, Quaternion.Euler(0f, 0f, angle));
            Projectile proj = proj_obj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.damage = defense_data != null ? defense_data.damage : 45;
                proj.splash_radius = 2.5f;
                proj.speed = 16f;
                proj.is_enemy_projectile = false;
            }
        }

        StartCoroutine(RecoilRoutine());

        if (ammo_current <= 0)
        {
            OnDestroyed();
        }
    }

    private IEnumerator RecoilRoutine()
    {
        if (visuals_container != null)
        {
            Vector3 orig_pos = visuals_container.transform.localPosition;
            float dir = transform.position.x < 0 ? 0.15f : -0.15f;
            visuals_container.transform.localPosition = orig_pos + new Vector3(dir, 0f, 0f);
            yield return new WaitForSeconds(0.08f);
            visuals_container.transform.localPosition = orig_pos;
        }
    }

    public void ReleaseBarrel()
    {
        if (is_rolling) return;
        is_rolling = true;
        roll_direction = transform.position.x < 0 ? -1f : 1f;
        if (prompt_text != null) prompt_text.gameObject.SetActive(false);
        Destroy(gameObject, 6f);
    }

    public void TriggerTrap()
    {
        if (defense_data == null) return;

        if (defense_data.defense_type == DefenseType.SingleUse)
        {
            ExplodeSingleUse();
        }
    }

    private void ExplodeSingleUse()
    {
        float radius = 2.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag("Enemy"))
            {
                Health enemy_health = hits[i].GetComponent<Health>();
                if (enemy_health != null && !enemy_health.is_dead)
                {
                    enemy_health.TakeDamage(defense_data != null ? defense_data.damage : 60);
                }
            }
        }
        OnDestroyed();
    }

    private void ExplodeBarrel()
    {
        float radius = 3.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag("Enemy"))
            {
                Health enemy_health = hits[i].GetComponent<Health>();
                if (enemy_health != null && !enemy_health.is_dead)
                {
                    enemy_health.TakeDamage(defense_data != null ? defense_data.damage : 80);
                }
            }
        }
        OnDestroyed();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (defense_data != null && defense_data.defense_type == DefenseType.SingleUse)
        {
            if (collision.CompareTag("Enemy") || collision.CompareTag("Projectile"))
            {
                TriggerTrap();
            }
        }
        else if (is_rolling)
        {
            if (collision.CompareTag("Enemy"))
            {
                ExplodeBarrel();
            }
        }
    }

    private void OnDestroyed()
    {
        Destroy(gameObject);
    }
}
