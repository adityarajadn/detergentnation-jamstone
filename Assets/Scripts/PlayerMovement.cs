using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform visualBody;
    public float moveSpeed = 4.5f;
    public float jumpForce = 9.0f;
    public float jetpackFuelMax = 100f;

    [HideInInspector] public float jetpackFuel = 100f;
    [HideInInspector] public float speedMultiplier = 1.0f;
    public float speedBoostTimer = 0f;

    public bool canDoubleJump = false;
    private bool hasDoubleJumped = false;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;

    public void SetupEquipment(string jetpackId)
    {
        canDoubleJump = (jetpackId == "joyride");
    }

    private void Update()
    {
        if (speedBoostTimer > 0f)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f) speedMultiplier = 1f;
        }

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer);
        }
        else
        {
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.05f;
        }

        if (isGrounded)
        {
            hasDoubleJumped = false;
            jetpackFuel = Mathf.Min(jetpackFuelMax, jetpackFuel + 40f * Time.deltaTime);
        }

        // Horizontal Movement
        float h = 0f;
        if (Input.GetKey(KeybindManager.MoveLeft) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeybindManager.MoveRight) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
        }
#endif
        h = Mathf.Clamp(h, -1f, 1f);

        rb.linearVelocity = new Vector2(h * moveSpeed * speedMultiplier, rb.linearVelocity.y);

        if (h != 0 && visualBody != null)
        {
            Vector3 s = visualBody.localScale;
            s.x = h > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            visualBody.localScale = s;
        }

        // Jump & Double Jump
        bool jumpDown = Input.GetKeyDown(KeybindManager.Jump) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool jumpHeld = Input.GetKey(KeybindManager.Jump) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
#if ENABLE_INPUT_SYSTEM
        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) jumpDown = true;
            if (kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed) jumpHeld = true;
        }
#endif

        if (jumpDown)
        {
            if (isGrounded)
            {
                float jmp = jumpForce;
                if (GameState.Instance != null && GameState.Instance.data.equippedJetpack == "vengeance") jmp *= 1.35f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jmp);
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                hasDoubleJumped = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.95f);
                AudioManager.Instance?.PlaySfx("shoot");
            }
        }

        // Jetpack Vengeance Slow-fall Glide
        if (jumpHeld && !isGrounded)
        {
            if (GameState.Instance != null && GameState.Instance.data.equippedJetpack == "vengeance" && jetpackFuel > 2f)
            {
                if (rb.linearVelocity.y < 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.MoveTowards(rb.linearVelocity.y, -1.0f, 15f * Time.deltaTime));
                    jetpackFuel -= 18f * Time.deltaTime;
                }
            }
        }
    }
}
