using System.Collections;
using UnityEngine;

public class DefenseTurret : DefenseObject
{
    [SerializeField] private GameObject projectile_prefab;
    private float next_cannon_fire_time = 0f;

    protected override bool CanInteract()
    {
        return ammo_current > 0;
    }

    protected override string GetInteractionPrompt(string keyName)
    {
        int max_ammo = defense_data != null ? defense_data.max_ammo : 5;
        return "[" + keyName + "] FIRE CANNON (" + ammo_current + "/" + max_ammo + ")";
    }

    protected override void OnInteract()
    {
        if (Time.time >= next_cannon_fire_time)
        {
            FireCannon();
        }
    }

    public void FireCannon()
    {
        if (ammo_current <= 0) return;

        ammo_current--;
        next_cannon_fire_time = Time.time + 0.5f;
        AudioManager.PlaySFX(GameConstants.AudioKeys.CannonFire);

        Vector2 fire_dir = transform.position.x < 0 ? Vector2.left : Vector2.right;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(GameConstants.Tags.Enemy);
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
        if (projectile_prefab != null)
        {
            GameObject proj_obj = Instantiate(projectile_prefab, transform.position + (Vector3)fire_dir * 0.8f + Vector3.up * 0.2f, Quaternion.Euler(0f, 0f, angle));
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
}
