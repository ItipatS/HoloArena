using UnityEngine;
using System.Collections;

public class FighterBlockModule : MonoBehaviour, ICharacterModule
{
    public BlockData_SO blockData; // Assigned via Inspector

    // Module references.
    private BlockVFXController blockVFX;
    private MovementModule movementModule;
    private PlayerInputModule inputModule;
    private GroundCheckModule groundCheck;
    private StatModule statModule;

    // Internal state flags.
    public bool IsBlocking { get; private set; } = false;
    private float blockPressTime = 0f;

    // Flag to signal that a block was requested.
    private bool blockRequested = false;

    private FighterController fighter;

    public void Initialize(FighterController controller)
    {
        blockVFX = controller.GetModule<BlockVFXController>();
        movementModule = controller.GetModule<MovementModule>();
        inputModule = controller.GetModule<PlayerInputModule>();
        groundCheck = controller.GetModule<GroundCheckModule>();
        statModule = controller.GetModule<StatModule>();
        fighter = GetComponent<FighterController>();
    }

    protected bool IsInAir => groundCheck != null && !groundCheck.IsGrounded;

    // Tick: runs every rendered frame. Capture input here.
    public void Tick()
    {
        HandleBlockInput();
    }

    private void HandleBlockInput()
    {
        // When the block key is pressed (key down) and we’re not already blocking, request a block.
        if (inputModule.IsKeyPressed("Block") && !IsBlocking && !IsInAir)
        {
            blockRequested = true;
        }

        // If we’re already blocking and the block key is held, update continuous block effects.
        if (IsBlocking && inputModule.IsKeyHeld("Block"))
        {
            ContinueBlock();
        }

        // Optionally, if blocking is active and the block key is released, signal to end block.
        if (IsBlocking && !inputModule.IsKeyHeld("Block"))
        {
            // We can initiate the recovery phase immediately.
            EndBlock();
        }
    }

    // FixedTick: process the block request on a fixed timestep.
    public void FixedTick()
    {
        if (blockRequested && !IsBlocking)
        {
            // Start the block routine in FixedTick.
            StartCoroutine(BlockRoutine());
            blockRequested = false;
        }
    }

    // This routine handles the startup and active phase.
    private IEnumerator BlockRoutine()
    {
        int startupFrames = blockData.startupFrames;
        int activeFrames = blockData.activeFrames;

        // --- Startup Phase ---
        movementModule.LockMovement(true, startupFrames);
        for (int i = 0; i < startupFrames; i++)
        {
            float t = (float)i / startupFrames;
            float sliderValue = Mathf.Lerp(-1f, 4f, t);
            blockVFX.blockMaterial.SetFloat("_MovingSlider", sliderValue);
            blockVFX.blockMaterial.SetColor("_TintColor", blockData.normalColor);
            yield return new WaitForFixedUpdate();
        }

        // Startup complete: activate block.
        IsBlocking = true;
        blockPressTime = Time.time;
        Debug.Log("Block active!");

        for (int i = 0; i < activeFrames; i++)
        {
            float consumption = blockData.staminaPerSecond * Time.deltaTime;
            if (!statModule.ConsumeStamina(consumption))
            {
                Debug.LogWarning("Not enough stamina; block canceled.");
                EndBlock();
                break;
            }
            movementModule.LockMovement(true);
            // Update VFX based on current stamina.
            float currentStamina = statModule.currentStats.currentStamina;
            float maxStamina = statModule.currentStats.maxStamina;
            float staminaRatio = currentStamina / maxStamina;
            Color updatedColor = Color.Lerp(Color.red, blockData.normalColor, staminaRatio);
            blockVFX.blockMaterial.SetColor("_TintColor", updatedColor);
            blockVFX.blockMaterial.SetFloat("_MovingSlider", 4f);
            yield return new WaitForFixedUpdate();
        }
        while (inputModule.IsKeyHeld("Block"))
        {
            yield return new WaitForFixedUpdate();
        }

        // When the block key is released, exit active phase and start recovery.
        yield return RecoveryRoutine();

        movementModule.UnlockMovement();
        IsBlocking = false;
        Debug.Log("Block ended.");
    }

    // Called from Tick every frame while block is active.
    private void ContinueBlock()
    {
        // Consume stamina for continuous blocking.
        float consumption = blockData.staminaPerSecond * Time.deltaTime;
        if (!statModule.ConsumeStamina(consumption))
        {
            Debug.LogWarning("Not enough stamina; block canceled.");
            EndBlock();
            return;
        }

        movementModule.LockMovement(true);

        // Update VFX based on current stamina.
        float currentStamina = statModule.currentStats.currentStamina;
        float maxStamina = statModule.currentStats.maxStamina;
        float staminaRatio = currentStamina / maxStamina;
        Color updatedColor = Color.Lerp(Color.red, blockData.normalColor, staminaRatio);
        blockVFX.blockMaterial.SetColor("_TintColor", updatedColor);
        blockVFX.blockMaterial.SetFloat("_MovingSlider", 4f);
    }

    // Ends the block immediately by starting the recovery phase.
    private void EndBlock()
    {
        // If already ending or not blocking, do nothing.
        if (!IsBlocking) return;

        // Stop the active block phase and begin recovery.
        StopAllCoroutines(); // Make sure any running BlockRoutine is halted.
        StartCoroutine(RecoveryRoutine());
        movementModule.UnlockMovement();
        IsBlocking = false;
        Debug.Log("Block forcefully ended.");
    }

    // Recovery routine for the block.
    private IEnumerator RecoveryRoutine()
    {
        int recoveryFrames = blockData.recoveryFrames;
        for (int i = 0; i < recoveryFrames; i++)
        {
            float t = (float)i / recoveryFrames;
            float sliderValue =  -1f;
            blockVFX.blockMaterial.SetFloat("_MovingSlider", sliderValue);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    public bool IsPerfectBlock()
    {
        // A perfect block is achieved if the block key is released within the perfectBlockWindow.
        return (Time.time - blockPressTime) <= blockData.perfectBlockThreshold;
    }
}
