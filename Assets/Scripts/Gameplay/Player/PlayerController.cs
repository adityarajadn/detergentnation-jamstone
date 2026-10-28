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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 3.5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        transform.localScale = Vector3.one;

        if (visuals == null) visuals = transform.Find("Visuals");

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
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        Health hp = GetComponent<Health>();
        if (hp != null)
        {
            hp.on_health_changed.AddListener((cur, max) => { if (cur < max && cur > 0) AudioManager.PlaySFX("player_hurt"); });
            hp.on_death.AddListener(() => AudioManager.PlaySFX("player_death"));
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
        // 1. Armor
        if (!string.IsNullOrEmpty(equipped_armor) && equipped_armor != GameConstants.EquipmentNames.None)
        {
            GameObject armor_prefab = FindPrefabByName(armor_prefabs, equipped_armor);
            if (armor_prefab != null && armor_mount != null)
            {
                GameObject armor_obj = Instantiate(armor_prefab, armor_mount);
                armor_obj.transform.localPosition = Vector3.zero;
                armor_obj.transform.localRotation = Quaternion.identity;
                active_armor = armor_obj.GetComponent<ArmorBase>();

                Health hp = GetComponent<Health>();
                if (hp != null) hp.SetArmor(active_armor);
            }
        }

        // 2. Jetpack
        if (!string.IsNullOrEmpty(equipped_jetpack) && equipped_jetpack != GameConstants.EquipmentNames.None)
        {
            GameObject jetpack_prefab = FindPrefabByName(jetpack_prefabs, equipped_jetpack);
            if (jetpack_prefab != null && jetpack_mount != null)
            {
                GameObject jetpack_obj = Instantiate(jetpack_prefab, jetpack_mount);
                jetpack_obj.transform.localPosition = Vector3.zero;
                jetpack_obj.transform.localRotation = Quaternion.identity;
                active_jetpack = jetpack_obj.GetComponent<JetpackBase>();
            }
        }
    }

    private GameObject FindPrefabByName(GameObject[] list, string match_name)
    {
        if (list == null) return null;
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == null) continue;
            string n = list[i].name.Replace("Armor_", "").Replace("Jetpack_", "").Replace("_", " ");
            if (n.Equals(match_name, System.StringComparison.OrdinalIgnoreCase) || list[i].name.Contains(match_name))
            {
                return list[i];
            }
            ArmorBase ab = list[i].GetComponent<ArmorBase>();
            if (ab != null && ab.armor_name == match_name) return list[i];
            JetpackBase jb = list[i].GetComponent<JetpackBase>();
            if (jb != null && jb.jetpack_name == match_name) return list[i];
        }
        return null;
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
        animator.SetFloat("speed", Mathf.Abs(move_input));
        animator.SetBool("is_grounded", is_grounded);
        if (rb != null)
        {
            animator.SetFloat("y_velocity", rb.linearVelocity.y);
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
    }

    private void ReadInput()
    {
        move_input = 0f;
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
                float boost = active_jetpack != null ? active_jetpack.GetJumpForceMultiplier() : (equipped_jetpack == GameConstants.EquipmentNames.JetpackJoyride ? joyride_speed_multiplier : (equipped_jetpack == GameConstants.EquipmentNames.JetpackVengeance ? vengeance_speed_multiplier : 1f));
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump_force * boost);
                can_double_jump = active_jetpack != null ? active_jetpack.AllowsDoubleJump() : (equipped_jetpack == GameConstants.EquipmentNames.JetpackJoyride);
            }
            else if (can_double_jump && (active_jetpack != null ? active_jetpack.AllowsDoubleJump() : (equipped_jetpack == GameConstants.EquipmentNames.JetpackJoyride)))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump_force * 1.05f);
                can_double_jump = false;
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
            can_double_jump = active_jetpack != null ? active_jetpack.AllowsDoubleJump() : (equipped_jetpack == GameConstants.EquipmentNames.JetpackJoyride);
        }
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
}
