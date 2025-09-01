using UnityEngine;
using System.Collections;
using System.Linq;

public class FighterAttackModule : ICharacterModule
{
    private readonly MoveProfile_SO moveProfile; // Contains both normal and combo moves.
    // Common module references.
    private readonly MovementModule movementModule;
    private readonly Animator animator;
    private readonly StatModule statModule;
    private readonly InputBufferModule inputBuffer;
    private readonly Hitbox hitbox;
    private readonly Hurtbox hurtbox;
    private readonly FighterFacade facade;
    private readonly System.Func<IEnumerator, Coroutine> coroutineRunner;

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
    // For ICharacterModule compatibility
    public void Initialize(FighterController controller) { /* Not used in this refactor, use constructor instead */ }

    protected bool IsBlocking => facade.IsBlocking;
    protected bool IsInAir => facade.IsInAir;

    private void HandleCharacterHit(GameObject hitCharacter, int hitStop)
    {
        // Use facade or inject a reference to the owning GameObject if needed
        if (facade != null && hitCharacter == facade.gameObject)
        {
            if (!IsBlocking)
            {
                CancelAttackRoutine();
            }
        }
    }
    // New constructor for dependency injection
    public FighterAttackModule(
        MoveProfile_SO moveProfile,
        MovementModule movementModule,
        Animator animator,
        StatModule statModule,
        InputBufferModule inputBuffer,
        Hitbox hitbox,
        Hurtbox hurtbox,
        FighterFacade facade,
        System.Func<IEnumerator, Coroutine> coroutineRunner)
    {
        this.moveProfile = moveProfile;
        this.movementModule = movementModule;
        this.animator = animator;
        this.statModule = statModule;
        this.inputBuffer = inputBuffer;
        this.hitbox = hitbox;
        this.hurtbox = hurtbox;
        this.facade = facade;
        this.coroutineRunner = coroutineRunner;
        if (this.hitbox != null) this.hitbox.Owner = facade != null ? facade.gameObject : null;
        if (this.hurtbox != null) this.hurtbox.Owner = facade != null ? facade.gameObject : null;
        HitEventManager.OnCharacterHit += HandleCharacterHit;
    }
    public void FixedTick(float fixedDeltaTime)
    {
        // Only allow new attacks if not already attacking and if on the ground.
        if (!IsAttacking && !IsBlocking)
        {
           if (statModule.currentStats.currentHealth <= 50f)
            {
                foreach (var specialSlot in moveProfile.moves.Where(s => s.category == AttackCategory.Special))
            {
                string[] specialSequence = specialSlot.attackData.specialInput.Select(x => x.ToString()).ToArray();
                if (inputBuffer.CheckCommandInput(specialSequence, 60f))
                {
                    Debug.LogWarning("Special combo triggered: " + specialSlot.moveName);
                    CurrentAttack = specialSlot.attackData;
                    StartAttack(CurrentAttack);
                }
            }
            }

            // Process combo input.
            if (ProcessComboInput())
            {
                AttackData_SO nextComboAttack = GetNextComboAttack();
                Debug.LogWarning("Combo chain attack: " + (nextComboAttack != null ? nextComboAttack.attackName : "None"));
                if (nextComboAttack != null)
                {
                    if (IsInAir && !nextComboAttack.AirAttack)
                    {
                        // Skip this attack or reset combo if necessary.
                        ResetCombo();
                    }
                    else
                    {
                      StartAttack(nextComboAttack);
                    }
                }
            }

            var moves = moveProfile.moves;
            for (int i = 0; i < moves.Count; i++)
            {
                var slot = moves[i];
               if (inputBuffer.ConsumeInput(slot.attackData.inputName.ToString()))
                {
                    // If in the air but this attack isn’t allowed, skip it.
                    if (IsInAir && !slot.attackData.AirAttack)
                        continue;

                    CurrentAttack = slot.attackData;

                    Debug.LogWarning("Normal attack triggered: " + slot.moveName);
                    StartAttack(CurrentAttack);
                }
            }
        }
        // Update the combo timer.
        if (comboTimer > 0f)
        {
            comboTimer = Mathf.Max(0f, comboTimer - fixedDeltaTime);
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
            if (combo.comboMoves == null || combo.comboMoves.Length == 0) continue;

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
    
    private void StartAttack(AttackData_SO attack)
    {
        if (attack == null || IsAttacking) return;
        CurrentAttack = attack;
        IsAttacking = true; 
        if (coroutineRunner != null) coroutineRunner(ExecuteAttack()); // Start once
    }

    // --- Attack Execution Methods ---
    private IEnumerator ExecuteAttack()
    {
        AttackData_SO attack = CurrentAttack;
        if (attack == null) yield break;

        if (!statModule.ConsumeStamina(attack.stamina)) // costs 20 stamina to attack
        {
            Debug.Log("Not enough stamina to attack!");
            yield break;
        }
        
        yield return AttackRoutine(attack);
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
            if (cancelAttack)
            {
                CancelAttackRoutine();
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }

        // Active phase: enable the hitbox.
        for (int i = 0; i < activeFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        // Recovery phase.
        for (int i = 0; i < recoveryFrames; i++)
        {
            if (cancelAttack)
            {
                CancelAttackRoutine();
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }

        IsAttacking = false;
        Debug.LogWarning("FinishAttacking!");
    }

    public void ActivateHitbox()
    {
        // Retrieve the Hitbox module instead of doing transform.Find("Hitbox")
        if (hitbox == null || CurrentAttack == null) return;

        Debug.Log("Activating Hitbox for " + CurrentAttack.attackName);
        hitbox.gameObject.SetActive(true);
        
        if (hitbox.TryGetComponent<BoxCollider>(out var collider))
        {
            collider.size = CurrentAttack.hitboxSize;
            var center = collider.center;
            center.z = (CurrentAttack.hitboxSize.z * 0.5f) + 0.5f; // rearOffset
            collider.center = center;
        }

        var fwd = (facade != null ? facade.gameObject.transform.forward : Vector3.forward);
        var kb = fwd * CurrentAttack.knockbackMagnitude + Vector3.up * CurrentAttack.knockbackVerticalFactor;

        hitbox.damage        = statModule.GetStat("currentattack"); // consider a stronger typed API
        hitbox.knockback     = kb;
        hitbox.activeDuration= CurrentAttack.activeFrames / 60f;
        hitbox.hitStop       = CurrentAttack.hitStop;
    }

    public void DeactivateHitbox()
    {
        if (hitbox == null) return;
        hitbox.gameObject.SetActive(false);
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
