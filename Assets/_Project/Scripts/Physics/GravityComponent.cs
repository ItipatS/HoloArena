using UnityEngine;
using System.Collections;

public class GravityComponent : MonoBehaviour, ICharacterModule
{
    private Rigidbody rb;
    private Animator animator;
    private GroundCheckModule groundCheck;
    private PlayerInputModule inputModule;
    private StatModule statModule;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float terminalVelocity = -50f;
    [SerializeField] private float fallGravityMultiplier = 2f;

    [Header("Jump Settings")]
    private bool isJumping;
    private float jumpHoldTimer;
    private bool jumpRequested;

    // New: To ignore ground check briefly after jump starts.
    private float jumpStartTime = -1f;
    [SerializeField] private float jumpIgnoreDuration = 0.1f; // duration in seconds

    [SerializeField] private float jumpCooldownDuration = 0.2f; 
    private float jumpCooldownTimer = 0f;

    public bool IsInAir => !groundCheck.IsGrounded;

    public void Initialize(FighterController controller)
    {
        animator = controller.Animator;
        rb = controller.Rigidbody;
        groundCheck = controller.GetModule<GroundCheckModule>();
        inputModule = controller.GetModule<PlayerInputModule>();
        statModule = controller.GetModule<StatModule>();
    }

    public void Tick()
    {
        jumpCooldownTimer = Mathf.Max(0, jumpCooldownTimer - Time.deltaTime);

        HandleJumpInput();
        if (groundCheck.IsGrounded && isJumping && (Time.time - jumpStartTime > jumpIgnoreDuration))
        {
            EndJump();
        }
    }

    public void FixedTick()
    {
        ApplyGravityAndJump();
    }

    private void HandleJumpInput()
    {
        if (inputModule.IsKeyPressed("Jump") && groundCheck.CanJump && jumpCooldownTimer <= 0f)
        {
            Debug.Log("Starting Jump!");
            jumpRequested = true;
        }

        if (inputModule.IsKeyHeld("Jump") && isJumping)
        {
            ContinueJump();
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpRequested = false;
        jumpHoldTimer = statModule.currentStats.jumpHoldDuration;
        jumpStartTime = Time.time;
        rb.velocity = new Vector3(rb.velocity.x, statModule.currentStats.jumpHoldForce, rb.velocity.z);

        jumpCooldownTimer = jumpCooldownDuration;
        
    }

    private void ContinueJump()
    {
        if (jumpHoldTimer > 0)
        {
            float boost = statModule.currentStats.jumpHoldForce * 0.5f * Time.fixedDeltaTime / statModule.currentStats.jumpHoldDuration;
            rb.AddForce(Vector3.up * boost, ForceMode.Acceleration);
            jumpHoldTimer -= Time.fixedDeltaTime;
        }
        else
        {
            EndJump();
        }
    }

    private void EndJump()
    {
        isJumping = false;
        jumpHoldTimer = 0f;
        jumpStartTime = -1f; // Reset jumpStartTime so that it doesn't interfere with future ground checks.
    }


    private void ApplyGravityAndJump()
    {
        if (jumpRequested)
        {
            StartJump();
        }

        float gravity = GravityManager.Instance.GetGravity(rb) * gravityScale;
        Vector3 velocity = rb.velocity;

        // Only apply grounded override if we're not in the jump grace period.
        if (groundCheck.IsGrounded && !isJumping && velocity.y <= 0 && (jumpStartTime < 0 || Time.fixedDeltaTime - jumpStartTime > jumpIgnoreDuration))
        {
            velocity.y = -0.1f;
        }
        else
        {
            if (velocity.y < 0 && !isJumping)
            {
                gravity *= fallGravityMultiplier;
            }

            velocity.y += gravity * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        }

        rb.velocity = velocity;
    }

    public void ApplyVerticalForce(float force, bool overrideVelocity = false)
    {
        if (overrideVelocity)
            rb.velocity = new Vector3(rb.velocity.x, force, rb.velocity.z);
        else
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }
}
