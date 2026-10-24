using UnityEngine;

public class JetpackJoyride : JetpackBase
{
    [SerializeField] private float jump_boost = 1.15f;

    private void Reset()
    {
        jetpack_name = GameConstants.EquipmentNames.JetpackJoyride;
    }

    protected override void Awake()
    {
        base.Awake();
        jetpack_name = GameConstants.EquipmentNames.JetpackJoyride;
    }

    public override float GetJumpForceMultiplier()
    {
        return jump_boost;
    }

    public override bool AllowsDoubleJump()
    {
        return true;
    }
}
