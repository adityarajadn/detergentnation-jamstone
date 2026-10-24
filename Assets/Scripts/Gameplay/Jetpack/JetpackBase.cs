using UnityEngine;

public class JetpackBase : MonoBehaviour
{
    [Header("Jetpack Configuration")]
    public string jetpack_name = "Jetpack";

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public virtual float GetJumpForceMultiplier()
    {
        return 1f;
    }

    public virtual bool AllowsDoubleJump()
    {
        return false;
    }

    public virtual void HandleGliding(Rigidbody2D rb, bool is_grounded) { }
}
