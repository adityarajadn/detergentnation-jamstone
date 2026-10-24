using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float move_speed = 6f;
    public float jump_force = 12.5f;
    public string equipped_jetpack = "None";

    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;
    [SerializeField] private float ground_radius = 0.22f;

    private Rigidbody2D rb;
    private bool is_grounded = false;
    private bool can_double_jump = false;
    private float move_input = 0f;
    private float speed_boost_multiplier = 1f;
    private Coroutine speed_boost_coroutine = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;

        if (ground_layer.value == 0)
        {
            ground_layer = LayerMask.GetMask("Ground", "Default", "OneWayPlatform");
            if (ground_layer.value == 0)
            {
                ground_layer = ~LayerMask.GetMask("Player", "Enemy", "Ignore Raycast");
            }
        }
    }

    private void Start()
    {
        if (GameManager.instance != null)
        {
            equipped_jetpack = GameManager.instance.equipped_jetpack;
        }
        else
        {
            equipped_jetpack = PlayerPrefs.GetString("equipped_jetpack", "None");
        }
    }

    private void Update()
    {
        ReadInput();
        HandleGliding();
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
    }

    private void ReadInput()
    {
        move_input = 0f;
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
        {
            move_input -= 1f;
        }
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
        {
            move_input += 1f;
        }

        if (kb.spaceKey.wasPressedThisFrame)
        {
            if (is_grounded)
            {
                float boost = equipped_jetpack == "Joyride" ? 1.1f : (equipped_jetpack == "I am Vengeance" ? 1.25f : 1f);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump_force * boost);
                can_double_jump = (equipped_jetpack == "Joyride");
            }
            else if (can_double_jump && equipped_jetpack == "Joyride")
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump_force * 1.05f);
                can_double_jump = false;
            }
        }

        if ((kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) && is_grounded)
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
            can_double_jump = (equipped_jetpack == "Joyride");
        }
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(move_input * move_speed * speed_boost_multiplier, rb.linearVelocity.y);

        if (move_input > 0.05f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (move_input < -0.05f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
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
        if (equipped_jetpack != "I am Vengeance" || is_grounded) return;

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

        int platform_layer = LayerMask.NameToLayer("OneWayPlatform");
        if (platform_layer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, platform_layer, true);
            yield return new WaitForSeconds(0.35f);
            Physics2D.IgnoreLayerCollision(gameObject.layer, platform_layer, false);
        }
    }
}
