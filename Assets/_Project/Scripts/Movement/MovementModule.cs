using System.Collections;
using UnityEngine;

public class MovementModule : MonoBehaviour, ICharacterModule
{
    private Rigidbody rb;
    private PlayerInputModule inputModule;
    private InputBufferModule buffer;
    private StatModule statModule;
    private GravityComponent gravityComponent;
    [SerializeField] private Transform opponent;
    private Camera mainCamera;

    private Vector3 moveInput; // Raw Input Direction
    private Vector3 moveVelocity; //Current velocity
    private float currentSpeed;
    private bool isSliding;
    private bool isDashing;
    public bool isMovementLocked { get; private set; }

    public float CurrentSpeed => currentSpeed;
    public bool IsSliding => isSliding;
    private float slideDuration = 0.5f;
    private float slideDeceleration = 10f;
    private float slideTimer;

    public bool IsDashing => isDashing;
    private float dashDuration = 0.2f;
    private float dashSpeed = 25f;
    private float dashTimer;
    private Vector3 dashDirection;
    private Coroutine movementLockCoroutine;


    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 6f;  // Slerp rotation speed
    [SerializeField] private float airControlMultiplier = 0.3f;

    public void Initialize(FighterController controller)
    {
        buffer = controller.InputBuffer;
        rb = controller.Rigidbody;
        inputModule = controller.GetModule<PlayerInputModule>();
        statModule = controller.GetModule<StatModule>();
        gravityComponent = controller.GetModule<GravityComponent>();
        mainCamera = Camera.main;
        currentSpeed = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (isMovementLocked) return;

        AssembleDashDirection();

        if (!isDashing)
        {
            moveInput = AssembleMoveDirection();
        }
    }

    public void FixedTick(float fixedDeltaTime)
    {
        if (isMovementLocked) return;

        if (isDashing)
        {
            dashTimer -= fixedDeltaTime;
            rb.velocity = new Vector3(dashDirection.x, 0, dashDirection.z) * dashSpeed + Vector3.up * rb.velocity.y;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        else if (!HasMovementInput && currentSpeed > 9f && !isSliding && !gravityComponent.IsInAir)
        {
            StartSliding();
        }
        else if (isSliding)
        {
            UpdateSliding(fixedDeltaTime);
        }
        else
        {
            UpdateMovement(fixedDeltaTime);
        }
        UpdateRotation(fixedDeltaTime);
    }
    private void StartSliding()
    {
        isSliding = true;
        slideTimer = slideDuration;
        // Notify state machine (e.g., via event or direct call)
    }

    private void UpdateSliding(float fixedDeltaTime)
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, slideDeceleration * fixedDeltaTime);
        moveVelocity = moveInput * currentSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        slideTimer -= fixedDeltaTime;
        if (slideTimer <= 0f || currentSpeed <= 0.1f)
        {
            isSliding = false;
            // Notify state machine to return to Idle
        }
    }

    private void AssembleDashDirection()
    {
        Vector3 direction = Vector3.zero;
        if (mainCamera == null) return;

        // Camera-relative movement
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0; // Keep movement horizontal
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();


        if (buffer.CheckCommandInput(new[] { "Right", "Right" }, 30f))
            StartDash(direction += cameraRight);
        else if (buffer.CheckCommandInput(new[] { "Left", "Left" }, 30f))
            StartDash(direction -= cameraRight);
        else if (buffer.CheckCommandInput(new[] { "Forward", "Forward" }, 30f))
            StartDash(direction += cameraForward);
        else if (buffer.CheckCommandInput(new[] { "Backward", "Backward" }, 30f))
            StartDash(direction -= cameraForward);

    }
    private Vector3 AssembleMoveDirection()
    {
        Vector3 direction = Vector3.zero;
        if (mainCamera == null) return direction;

        // Camera-relative movement
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0; // Keep movement horizontal
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        if (inputModule.IsKeyHeld("Right"))
            direction += cameraRight;
        if (inputModule.IsKeyHeld("Left"))
            direction -= cameraRight;
        if (inputModule.IsKeyHeld("Forward"))
            direction += cameraForward;
        if (inputModule.IsKeyHeld("Backward"))
            direction -= cameraForward;

        return direction.normalized;
    }

    private void StartDash(Vector3 direction)
    {
        if (!statModule.ConsumeStamina(20f))
            return;

        Debug.LogWarning("StartDashing: " + direction);
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction.normalized;
        AudioManager.Instance.PlaySound("Dash");
        // Optional: play dash VFX or sound here
    }


    private void UpdateMovement(float fixedDeltaTime)
    {
        float targetSpeed = moveInput != Vector3.zero ? statModule.GetStat("speed") : 0f;

        if (gravityComponent.IsInAir)
            targetSpeed *= airControlMultiplier;

        float accel = moveInput != Vector3.zero ? statModule.GetStat("acceleration") : statModule.GetStat("deceleration");
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * fixedDeltaTime);

        moveVelocity = moveInput * currentSpeed;

        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    private void UpdateRotation(float fixedDeltaTime)
    {
        Vector3 horizontalInput = new Vector3(moveInput.x, 0, moveInput.z);

        if (horizontalInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * fixedDeltaTime));
        }
        else
        {
            AutoFaceOpponent(opponent, fixedDeltaTime);
        }
    }

    // Public methods for state control (e.g., from attack animations)
    public void LockMovement(bool lockState, int duration = 0)
    {
        Debug.LogError("Lock Movement Duration: " + duration);
        // If we're locking movement, start the coroutine.
        if (lockState)
        {
            // Cancel any existing lock coroutine.
            if (movementLockCoroutine != null)
            {
                StopCoroutine(movementLockCoroutine);
            }

            isMovementLocked = true;
            moveInput = Vector3.zero;
            currentSpeed = 0f;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            if (duration > 0)
            {
                movementLockCoroutine = StartCoroutine(MovementLockCoroutine(duration));
            }
        }
        else
        {
            // Unlock movement immediately. 
            UnlockMovement();
        }
    }

    private IEnumerator MovementLockCoroutine(int duration)
    {
        for (int i = 0; i < duration; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        UnlockMovement();
    }

    public void UnlockMovement()
    {
        isMovementLocked = false;
        movementLockCoroutine = null;
    }
    // For debugging or external queries
    public Vector3 GetMoveDirection() => moveInput;

    public bool HasMovementInput => inputModule.IsKeyHeld("Right") || inputModule.IsKeyHeld("Left") ||
                                   inputModule.IsKeyHeld("Forward") || inputModule.IsKeyHeld("Backward");

    private void AutoFaceOpponent(Transform opponent, float fixedDeltaTime)
    {
        if (opponent == null)
            return;

        // Calculate horizontal direction from character to opponent.
        Vector3 targetDirection = opponent.position - transform.position;
        targetDirection.y = 0;

        // Avoid jitter if the target is extremely close.
        if (targetDirection.sqrMagnitude < 0.001f)
            return;

        // Determine target rotation.
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Calculate the maximum rotation allowed this FixedUpdate.
        float maxDegreesDelta = 270f * fixedDeltaTime;

        // Smoothly rotate towards the target rotation.
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, maxDegreesDelta);

        // Apply the new rotation.
        rb.MoveRotation(newRotation);

        // Reset angular velocity to prevent unwanted residual rotation from physics collisions.
        rb.angularVelocity = Vector3.zero;
    }

    public void SetOpponent(Transform opponentTransform)
    {
        opponent = opponentTransform;
    }

}
