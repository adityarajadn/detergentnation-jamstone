using DG.Tweening;
using UnityEngine;

public class DefenseTurret : DefenseObject
{
    private const float RecoilDistance = 0.15f;
    private const float RecoilKickSeconds = 0.03f;
    private const float RecoilReturnSeconds = 0.12f;

    [SerializeField] private GameObject projectile_prefab;
    private float next_cannon_fire_time = 0f;
    private Tween recoil_tween;
    private float recoil_origin_x;
    private bool recoil_origin_set;

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

    private Animator animator;
    private SpriteRenderer sprite_renderer;
    private static readonly int AnimParamFire = Animator.StringToHash("Fire");

    protected override void Awake()
    {
        base.Awake();
        if (visuals_container != null)
        {
            animator = visuals_container.GetComponent<Animator>();
            sprite_renderer = visuals_container.GetComponent<SpriteRenderer>();
        }
    }

    public override void SetFacingDirection(FacingDirection direction)
    {
        base.SetFacingDirection(direction);
        if (sprite_renderer != null)
        {
            sprite_renderer.flipX = direction == FacingDirection.Left;
        }
    }

    public void FireCannon()
    {
        if (ammo_current <= 0) return;

        ammo_current--;
        next_cannon_fire_time = Time.time + 0.5f;
        AudioManager.PlaySFX(GameConstants.AudioKeys.CannonFire);

        // Default orientation comes from slot direction
        Vector2 default_dir = facing_direction == FacingDirection.Left ? Vector2.left : Vector2.right;
        Vector2 fire_dir = default_dir;
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

        bool facing_left = fire_dir.x < 0f;
        if (sprite_renderer != null)
        {
            sprite_renderer.flipX = facing_left;
        }

        if (animator != null)
        {
            animator.SetTrigger(AnimParamFire);
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

        PlayRecoil(facing_left);

        if (ammo_current <= 0)
        {
            OnDestroyed();
        }
    }

    private void PlayRecoil(bool facing_left)
    {
        if (visuals_container == null) return;

        Transform visual = visuals_container.transform;
        if (!recoil_origin_set)
        {
            recoil_origin_x = visual.localPosition.x;
            recoil_origin_set = true;
        }

        KillRecoil();
        visual.localPosition = new Vector3(recoil_origin_x, visual.localPosition.y, visual.localPosition.z);

        // Recoil opposite to facing direction
        float dir = facing_left ? RecoilDistance : -RecoilDistance;
        recoil_tween = DOTween.Sequence()
            .Append(visual.DOLocalMoveX(recoil_origin_x + dir, RecoilKickSeconds))
            .Append(visual.DOLocalMoveX(recoil_origin_x, RecoilReturnSeconds).SetEase(Ease.OutQuad));
    }

    private void KillRecoil()
    {
        if (recoil_tween != null)
        {
            recoil_tween.Kill();
            recoil_tween = null;
        }
    }

    private void OnDisable()
    {
        KillRecoil();
    }

    protected override void OnDestroy()
    {
        KillRecoil();
        base.OnDestroy();
    }
}
