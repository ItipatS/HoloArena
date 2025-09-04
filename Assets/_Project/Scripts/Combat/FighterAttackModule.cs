using UnityEngine;
using System.Collections;
using System.Linq;

public class FighterAttackModule : MonoBehaviour, ICharacterModule
{
    [SerializeField] private MoveProfile_SO moveProfile; // Contains both normal and combo moves.
    // Common module references.
    private MovementModule movementModule;
    private Animator animator;
    private StatModule statModule;
    private InputBufferModule inputBuffer;
    private Hitbox hitbox;
    private Hurtbox hurtbox;
    private FighterFacade facade;

    // Attack state.
    public bool IsAttacking { get; private set; }
    private bool cancelAttack = false;
    public AttackData_SO CurrentAttack { get; private set; }

    // --- Combo Management Fields ---
    // These fields replace the separate ComboAttackManager.
    private ComboAttackData_SO currentCombo = null; // The current combo chain in progress.
    private int currentComboIndex = 0;               // Index for the current hit in the combo chain.
    private float comboTimer = 0f;                     // Timer to reset the combo if no input is received.

    // The combo window is taken from the first combo in the moveProfile.
    private float CurrentComboWindow
    {
        get
        {
            if (moveProfile.comboAttacks != null && moveProfile.comboAttacks.Count > 0)
                return moveProfile.comboAttacks[0].comboWindow;
            return 0.5f; // Fallback value.
        }
    }

    // --- ICharacterModule Initialization ---
    public void Initialize(FighterController controller)
    {
        hitbox = GetComponentInChildren<Hitbox>(true);
        hurtbox = GetComponentInChildren<Hurtbox>(true);
        movementModule = controller.GetModule<MovementModule>();
        facade = controller.GetModule<FighterFacade>();
        statModule = controller.GetModule<StatModule>();
        animator = controller.Animator;
        inputBuffer = controller.InputBuffer;
        hitbox.Owner = gameObject;
        hurtbox.Owner = gameObject;
        HitEventManager.OnCharacterHit += HandleCharacterHit;
    }

    protected bool IsBlocking => facade.IsBlocking;
    protected bool IsInAir => facade.IsInAir;

    private void HandleCharacterHit(GameObject hitCharacter, int hitStop)
    {
        if (hitCharacter == gameObject)
        {
            if (!IsBlocking)
            {
                CancelAttackRoutine();
            }

        }
    }

    // --- Continuous Updates ---
    public void FixedTick(float fixedDeltaTime)
    {
        // Only allow new attacks if not already attacking and if on the ground.
        if (!IsAttacking && !IsBlocking)
        {
            if (statModule.currentStats.currentHealth <= 50f)
            {
                // Check for special moves first.
                foreach (var specialSlot in moveProfile.moves.Where(s => s.category == AttackCategory.Special))
                {
                    string[] specialSequence = specialSlot.attackData.specialInput.Select(x => x.ToString()).ToArray();
                    if (inputBuffer.CheckCommandInput(specialSequence, specialSlot.attackData.specialComboMaxGap, consume: true))
                    {
                        Debug.LogWarning("Special combo triggered: " + specialSlot.moveName);
                        CurrentAttack = specialSlot.attackData;
                        StartCoroutine(ExecuteAttack());
                        return;
                    }
                }
            }

            // Process combo input.
                if (ProcessComboInput())
                {
                    CurrentAttack = GetNextComboAttack();
                    Debug.LogWarning("Combo chain attack: " + (CurrentAttack != null ? CurrentAttack.attackName : "None"));
                    if (CurrentAttack != null)
                    {
                        if (IsInAir && !CurrentAttack.AirAttack)
                        {
                            // Skip this attack or reset combo if necessary.
                            ResetCombo();
                        }
                        else
                        {
                            StartCoroutine(ExecuteAttack());
                            return;
                        }
                    }
                }

            foreach (var slot in moveProfile.moves.Where(s => s.category != AttackCategory.Special))
            {
                // Check if this attack should be available in the current state (air or ground)
                if (inputBuffer.ConsumeInput(slot.attackData.inputName.ToString()))
                {
                    // If in the air but this attack isn’t allowed, skip it.
                    if (IsInAir && !slot.attackData.AirAttack)
                        continue;

                    CurrentAttack = slot.attackData;
                    
                    Debug.LogWarning("Normal attack triggered: " + slot.moveName);
                    StartCoroutine(ExecuteAttack());
                    return;
                }
            }
        }
        // Update the combo timer.
        if (comboTimer > 0f)
        {
            comboTimer = Mathf.Max(0f, comboTimer - Time.deltaTime);
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void Tick(float deltaTime) { }

    private bool ProcessComboInput()
    {

        // If a combo chain is already in progress, check if the pressed input matches the expected next move.
        if (currentCombo != null)
        {
            AttackData_SO expectedMove = currentCombo.comboMoves[currentComboIndex];
            string expectedInput = expectedMove.inputName.ToString();
            if (inputBuffer.ConsumeInput(expectedInput))
            {
                OnAttackInput();
                return true;
            }
        }

        // No combo in progress or previous combo was reset.
        // Check all combo attacks to see if the pressed input matches the first move.
        foreach (var combo in moveProfile.comboAttacks)
        {
            if (combo.comboMoves != null && combo.comboMoves.Length > 0)
            {
                string firstInput = combo.comboMoves[0].inputName.ToString();
                if (inputBuffer.ConsumeInput(firstInput))
                {
                    // Start a new combo chain.
                    currentCombo = combo;
                    currentComboIndex = 0;
                    OnAttackInput();
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Called when a valid combo input is received.
    /// Advances the combo chain and resets the timer.
    /// </summary>
    private void OnAttackInput()
    {
        if (comboTimer > 0f)
        {
            // Within the combo window, advance the combo.
            currentComboIndex++;
        }
        else
        {
            // Otherwise, start a new combo.
            currentComboIndex = 0;
        }
        comboTimer = CurrentComboWindow;
    }

    // Returns the next attack data in the current combo chain.
    private AttackData_SO GetNextComboAttack()
    {
        if (currentCombo == null || currentCombo.comboMoves == null || currentCombo.comboMoves.Length == 0)
            return null;

        // Wrap around if index exceeds available moves.
        if (currentComboIndex >= currentCombo.comboMoves.Length)
            currentComboIndex = 0;
        return currentCombo.comboMoves[currentComboIndex];
    }

    // Resets the current combo chain.
    private void ResetCombo()
    {
        currentCombo = null;
        currentComboIndex = 0;
        comboTimer = 0f;
    }

    // --- Attack Execution Methods ---
    private IEnumerator ExecuteAttack()
    {
        if (statModule.ConsumeStamina(CurrentAttack.stamina)) // costs 20 stamina to attack
        {
            // Proceed with the attack.
            yield return StartCoroutine(AttackRoutine(CurrentAttack));
        }
        else
        {
            // Optionally provide feedback that stamina is too low.
            Debug.Log("Not enough stamina to attack!");
        }
    }

    private IEnumerator AttackRoutine(AttackData_SO attackData)
    {
        IsAttacking = true;
        Debug.LogWarning("Attacking!");
        int startupFrames = attackData.startupFrames;
        int activeFrames = attackData.activeFrames;
        int recoveryFrames = attackData.recoveryFrames;

        int totalAttackTime = attackData.startupFrames + attackData.activeFrames + attackData.recoveryFrames;
        movementModule.LockMovement(true, totalAttackTime);

        // Trigger the appropriate animation.
        animator.SetTrigger(attackData.animationTrigger);

        AudioManager.Instance.PlaySound("Grunt");

        for (int i = 0; i < startupFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (cancelAttack)
        {
            CancelAttackRoutine();
            yield break;
        }

        // Active phase: enable the hitbox.
        for (int i = 0; i < activeFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (cancelAttack)
        {
            CancelAttackRoutine();
            yield break;
        }

        // Recovery phase.
        for (int i = 0; i < recoveryFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (cancelAttack)
        {
            CancelAttackRoutine();
            yield break;
        }

        IsAttacking = false;
        Debug.LogWarning("FinishAttacking!");
    }

    public void ActivateHitbox()
    {
        // Retrieve the Hitbox module instead of doing transform.Find("Hitbox")
        if (hitbox != null)
        {
            Debug.Log("Activating Hitbox for " + CurrentAttack.attackName);

            StartCoroutine(DelayedActivate());

            IEnumerator DelayedActivate()
            {
                yield return new WaitForFixedUpdate();
                hitbox.gameObject.SetActive(true);
            }

            // Get the BoxCollider from the Hitbox component.
            BoxCollider collider = hitbox.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.size = CurrentAttack.hitboxSize;
                float rearOffset = 0.5f;
                Vector3 center = collider.center;
                center.z = (CurrentAttack.hitboxSize.z / 2f) + rearOffset;
                collider.center = center;
            }

            Vector3 knockbackDirection = transform.forward;  // You could adjust this if needed.
            Vector3 finalKnockback = knockbackDirection * CurrentAttack.knockbackMagnitude + Vector3.up * CurrentAttack.knockbackVerticalFactor;

            // Now update the Hitbox properties.
            hitbox.damage = statModule.GetStat("currentattack");
            hitbox.knockback = finalKnockback;
            hitbox.activeDuration = CurrentAttack.activeFrames / 60f;
            hitbox.hitStop = CurrentAttack.hitStop;
        }
    }

    public void DeactivateHitbox()
    {
        if (hitbox != null)
        {
            hitbox.gameObject.SetActive(false);
        }
    }

    private void CancelAttackRoutine()
    {
        DeactivateHitbox();
        IsAttacking = false;
        cancelAttack = false; // Reset cancel flag.
        movementModule.LockMovement(false);
    }

    private void OnDestroy()
    {
        HitEventManager.OnCharacterHit -= HandleCharacterHit;
        // Unsubscribe from any other events as well.
    }

}
