using UnityEngine;
using UnityEngine.InputSystem;

public class JetpackIAmVengeance : JetpackBase
{
    [SerializeField] private float launch_boost = 1.35f;
    [SerializeField] private float max_fall_speed = -2.2f;
    [SerializeField] private Animator animator;

    private static readonly int AnimParamIsGliding = Animator.StringToHash("IsGliding");

    private void Reset()
    {
        jetpack_name = GameConstants.EquipmentNames.JetpackVengeance;
    }

    protected override void Awake()
    {
        base.Awake();
        jetpack_name = GameConstants.EquipmentNames.JetpackVengeance;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public override float GetJumpForceMultiplier()
    {
        return launch_boost;
    }

    public override void HandleGliding(Rigidbody2D rb, bool is_grounded)
    {
        if (is_grounded || rb == null)
        {
            SetGliding(false);
            return;
        }

        Keyboard kb = Keyboard.current;
        bool wants_glide = (KeybindManager.IsPressed(KeyAction.Jump) || (kb != null && kb.spaceKey.isPressed)) && rb.linearVelocity.y < 0f;

        if (wants_glide)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, max_fall_speed));
            SetGliding(true);
        }
        else
        {
            SetGliding(false);
        }
    }

    private void SetGliding(bool gliding)
    {
        if (animator != null && animator.isInitialized)
        {
            animator.SetBool(AnimParamIsGliding, gliding);
        }
    }

    private void OnDisable()
    {
        SetGliding(false);
    }
}
