using UnityEngine;

public class JetpackJoyride : JetpackBase
{
    [SerializeField] private float jump_boost = 1.15f;
    [SerializeField] private GameObject thruster_fire;

    private Coroutine thruster_coroutine;

    private void Reset()
    {
        jetpack_name = GameConstants.EquipmentNames.JetpackJoyride;
    }

    protected override void Awake()
    {
        base.Awake();
        jetpack_name = GameConstants.EquipmentNames.JetpackJoyride;
        if (thruster_fire == null)
        {
            Transform t = transform.Find("ThrusterFire");
            if (t != null) thruster_fire = t.gameObject;
        }
        if (thruster_fire != null)
        {
            thruster_fire.SetActive(false);
        }
    }

    public override float GetJumpForceMultiplier()
    {
        return jump_boost;
    }

    public override bool AllowsDoubleJump()
    {
        return true;
    }

    public override void OnJumpTriggered()
    {
        if (thruster_fire == null) return;

        if (thruster_coroutine != null)
        {
            StopCoroutine(thruster_coroutine);
        }
        thruster_coroutine = StartCoroutine(ShowThrusterCoroutine());
    }

    private System.Collections.IEnumerator ShowThrusterCoroutine()
    {
        thruster_fire.SetActive(true);
        yield return new WaitForSeconds(0.45f);
        thruster_fire.SetActive(false);
        thruster_coroutine = null;
    }

    private void OnDisable()
    {
        if (thruster_coroutine != null)
        {
            StopCoroutine(thruster_coroutine);
            thruster_coroutine = null;
        }
        if (thruster_fire != null)
        {
            thruster_fire.SetActive(false);
        }
    }
}
