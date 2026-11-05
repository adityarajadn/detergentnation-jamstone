using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float move_speed = 6f;
    public float jump_force = 12.5f;
    public string equipped_jetpack = GameConstants.EquipmentNames.None;
    public string equipped_armor = GameConstants.EquipmentNames.None;

    [Header("Equipment Prefabs")]
    public GameObject[] armor_prefabs;
    public GameObject[] jetpack_prefabs;

    [Header("Active Equipment")]
    public ArmorBase active_armor;
    public JetpackBase active_jetpack;

    [Header("Mount Points")]
    [SerializeField] private Transform ground_check;
    [SerializeField] private Transform visuals;
    [SerializeField] private Transform armor_mount;
    [SerializeField] private Transform jetpack_mount;
    [SerializeField] private Transform hand_mount;

    [Header("Jetpack Modifiers")]
    [SerializeField] private float joyride_speed_multiplier = 1.1f;
    [SerializeField] private float vengeance_speed_multiplier = 1.25f;

    [SerializeField] private LayerMask ground_layer;
    [SerializeField] private float ground_radius = 0.22f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool is_grounded = false;
    private bool can_double_jump = false;
    private float move_input = 0f;
    public float MoveInput => move_input;
    private float speed_boost_multiplier = 1f;
    private Coroutine speed_boost_coroutine = null;
    private bool is_dying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ServiceLocator.Register<PlayerController>(this);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 3.5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        transform.localScale = Vector3.one;

        if (visuals == null) visuals = transform.Find("Visuals");
        if (hand_mount == null && visuals != null) hand_mount = visuals.Find("HandMount");
        if (hand_mount == null) hand_mount = transform.Find("HandMount");

        if (ground_layer.value == 0)
        {
            ground_layer = LayerMask.GetMask(GameConstants.Layers.Ground, GameConstants.Layers.Default, GameConstants.Layers.OneWayPlatform);
            if (ground_layer.value == 0)
            {
                ground_layer = ~LayerMask.GetMask(GameConstants.Layers.Player, GameConstants.Layers.Enemy, GameConstants.Layers.IgnoreRaycast);
            }
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerController>();
        if (Instance == this) Instance = null;
    }

    private int last_health = -1;
    private Coroutine hit_disable_coroutine = null;

    private void Start()
    {
        Health hp = GetComponent<Health>();
        if (hp != null)
        {
            last_health = hp.health_current;
            hp.on_health_changed.AddListener((cur, max) =>
            {
                if (last_health > 0 && cur < last_health && cur > 0)
                {
                    AudioManager.PlaySFX(GameConstants.AudioKeys.PlayerHurt);
                    if (animator != null) animator.SetTrigger(GameConstants.AnimParams.Hit);
                    if (hit_disable_coroutine != null) StopCoroutine(hit_disable_coroutine);
                    hit_disable_coroutine = StartCoroutine(DisableHandMountOnHit(0.25f));
                }
                last_health = cur;
            });
            hp.on_death.AddListener(() => AudioManager.PlaySFX(GameConstants.AudioKeys.PlayerDeath));
        }

        if (GameManager.instance != null)
        {
            equipped_jetpack = GameManager.instance.equipped_jetpack;
            equipped_armor = GameManager.instance.equipped_armor;
        }

        InitializeEquipment();
    }

    private void InitializeEquipment()
    {
        Health hp = GetComponent<Health>();

        if (PlayerEquipmentSpawner.TrySpawnArmor(equipped_armor, armor_prefabs, armor_mount, hp, out ArmorBase spawned_armor))
        {
            active_armor = spawned_armor;
        }

        if (PlayerEquipmentSpawner.TrySpawnJetpack(equipped_jetpack, jetpack_prefabs, jetpack_mount, out JetpackBase spawned_jetpack))
        {
            active_jetpack = spawned_jetpack;
        }
    }

    private void Update()
    {
        ReadInput();
        HandleGliding();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat(GameConstants.AnimParams.Speed, Mathf.Abs(move_input));
        animator.SetBool(GameConstants.AnimParams.IsGrounded, is_grounded);
        if (rb != null)
        {
            animator.SetFloat(GameConstants.AnimParams.YVelocity, rb.linearVelocity.y);
        }
    }

    private void FixedUpdate()
    {
        if (is_dying) return;
        CheckGround();
        ApplyMovement();
    }

    private void ReadInput()
    {
        move_input = 0f;
        if (is_dying) return;
        if (Time.timeScale == 0f) return;
        if (LevelManager.instance != null)
        {
            GamePhase phase = LevelManager.instance.current_phase;
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat) return;
        }

        Keyboard kb = Keyboard.current;

        if (KeybindManager.IsPressed(KeyAction.MoveLeft) || (kb != null && kb.leftArrowKey.isPressed))
        {
            move_input -= 1f;
        }
        if (KeybindManager.IsPressed(KeyAction.MoveRight) || (kb != null && kb.rightArrowKey.isPressed))
        {
            move_input += 1f;
        }

        if (KeybindManager.WasPressedThisFrame(KeyAction.Jump))
        {
            if (is_grounded)
            {
                float boost = GetJumpBoostMultiplier();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump_force * boost);
                can_double_jump = CanDoubleJumpWithEquipment();
            }
            else if (can_double_jump && CanDoubleJumpWithEquipment())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump_force * 1.05f);
                can_double_jump = false;
                AudioManager.PlaySFX(GameConstants.AudioKeys.Jetpack);
            }
        }

        if ((KeybindManager.WasPressedThisFrame(KeyAction.DropPlatform) || (kb != null && kb.downArrowKey.wasPressedThisFrame)) && is_grounded)
        {
            StartCoroutine(DropThroughPlatformCoroutine());
        }
    }

    private void CheckGround()
    {
        bool was_grounded = is_grounded;
        if (ground_check != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(ground_check.position, ground_radius, ground_layer);
            is_grounded = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !colliders[i].isTrigger && colliders[i].gameObject != gameObject)
                {
                    is_grounded = true;
                    break;
                }
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, ground_layer);
            is_grounded = hit.collider != null && !hit.collider.isTrigger;
        }

        if (is_grounded && !was_grounded)
        {
            can_double_jump = CanDoubleJumpWithEquipment();
        }
    }

    private float GetJumpBoostMultiplier()
    {
        return active_jetpack != null ? active_jetpack.GetJumpForceMultiplier() : 1f;
    }

    private bool CanDoubleJumpWithEquipment()
    {
        return active_jetpack != null && active_jetpack.AllowsDoubleJump();
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(move_input * move_speed * speed_boost_multiplier, rb.linearVelocity.y);
        transform.localScale = Vector3.one;

        if (visuals != null && GetComponent<PlayerWeaponAim>() == null)
        {
            if (move_input > 0.05f)
            {
                visuals.localScale = new Vector3(Mathf.Abs(visuals.localScale.x), visuals.localScale.y, visuals.localScale.z);
            }
            else if (move_input < -0.05f)
            {
                visuals.localScale = new Vector3(-Mathf.Abs(visuals.localScale.x), visuals.localScale.y, visuals.localScale.z);
            }
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speed_boost_coroutine != null)
        {
            StopCoroutine(speed_boost_coroutine);
        }
        speed_boost_coroutine = StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        speed_boost_multiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speed_boost_multiplier = 1f;
        speed_boost_coroutine = null;
    }

    private void HandleGliding()
    {
        if (Time.timeScale == 0f) return;
        if (LevelManager.instance != null)
        {
            GamePhase phase = LevelManager.instance.current_phase;
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat) return;
        }

        if (active_jetpack != null)
        {
            active_jetpack.HandleGliding(rb, is_grounded);
            return;
        }

        if (equipped_jetpack != GameConstants.EquipmentNames.JetpackVengeance || is_grounded) return;

        Keyboard kb = Keyboard.current;
        if (kb != null && kb.spaceKey.isPressed && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -2.2f));
        }
    }

    private IEnumerator DropThroughPlatformCoroutine()
    {
        Collider2D player_col = GetComponent<Collider2D>();
        if (player_col == null) yield break;

        int platform_layer = LayerMask.NameToLayer(GameConstants.Layers.OneWayPlatform);
        if (platform_layer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, platform_layer, true);
            yield return new WaitForSeconds(0.35f);
            Physics2D.IgnoreLayerCollision(gameObject.layer, platform_layer, false);
        }
    }

    private IEnumerator DisableHandMountOnHit(float duration)
    {
        if (hand_mount != null) hand_mount.gameObject.SetActive(false);
        yield return new WaitForSeconds(duration);
        if (hand_mount != null) hand_mount.gameObject.SetActive(true);
        hit_disable_coroutine = null;
    }

    public void TriggerDeathSequence(Action on_complete)
    {
        if (is_dying) return;
        is_dying = true;

        if (hit_disable_coroutine != null)
        {
            StopCoroutine(hit_disable_coroutine);
            hit_disable_coroutine = null;
        }

        StartCoroutine(PlayerDeathSequence.Run(this, rb, animator, active_armor, armor_mount, active_jetpack, jetpack_mount, hand_mount, on_complete));
    }
}
