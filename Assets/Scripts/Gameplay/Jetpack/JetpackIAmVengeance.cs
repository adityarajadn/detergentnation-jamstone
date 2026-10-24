using UnityEngine;
using UnityEngine.InputSystem;

public class JetpackIAmVengeance : JetpackBase
{
    [SerializeField] private float launch_boost = 1.35f;
    [SerializeField] private float max_fall_speed = -2.2f;

    private void Reset()
    {
        jetpack_name = GameConstants.EquipmentNames.JetpackVengeance;
    }

    protected override void Awake()
    {
        base.Awake();
        jetpack_name = GameConstants.EquipmentNames.JetpackVengeance;
    }

    public override float GetJumpForceMultiplier()
    {
        return launch_boost;
    }

    public override void HandleGliding(Rigidbody2D rb, bool is_grounded)
    {
        if (is_grounded || rb == null) return;

        Keyboard kb = Keyboard.current;
        if ((KeybindManager.IsPressed(KeyAction.Jump) || (kb != null && kb.spaceKey.isPressed)) && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, max_fall_speed));
        }
    }
}
