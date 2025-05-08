using UnityEngine;

public class GroundCheckModule : MonoBehaviour, ICharacterModule
{
    private CapsuleCollider capsuleCollider;
    public LayerMask groundLayer;

    [Header("Ground Check Settings")]
    public bool ShowGizmos = true;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float coyoteTime = 0.1f;
    private float lastGroundedTime;
    private Animator animator;

    public bool IsGrounded => isGrounded;
    public bool CanJump => Time.time - lastGroundedTime <= coyoteTime;

    public void Initialize(FighterController controller)
    {
        animator = controller.Animator;
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");

        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void Tick() { }

    public void FixedTick()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        //get the radius of the players capsule collider, and make it a tiny bit smaller than that
        float radius = capsuleCollider.radius * 0.9f;
        //get the position (assuming its right at the bottom) and move it up by almost the whole radius
        Vector3 pos = transform.position + Vector3.up * (radius * 0.9f);
        //returns true if the sphere touches something on that layer
        isGrounded = Physics.CheckSphere(pos, radius, groundLayer);

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        animator?.SetBool("IsInAir", !isGrounded);
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos || capsuleCollider == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        float radius = capsuleCollider.radius * 0.9f;
        Vector3 pos = transform.position + Vector3.up * (radius * 0.9f);
        Gizmos.DrawWireSphere(pos, radius);
    }
}
