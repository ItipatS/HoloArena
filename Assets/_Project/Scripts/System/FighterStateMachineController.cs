using System.Collections;
using UnityEngine;
using MonsterLove.StateMachine;

public class FighterStateMachineController : MonoBehaviour, ICharacterModule
{
    private StateMachine<FighterState, Driver> FighterSM;
    private Animator animator;
    private FighterFacade facade;
    private PlayerInputModule inputModule;

    private float minWalkSpeed = 0.1f;
    private float stopThreshold = 9f;

    public void Initialize(FighterController controller)
    {
        animator = controller.Animator;

        facade = controller.GetModule<FighterFacade>();
        inputModule = controller.GetModule<PlayerInputModule>();

        HitEventManager.OnCharacterHit += HandleCharacterHit;

        FighterSM = new StateMachine<FighterState, Driver>(this);
        FighterSM.ChangeState(FighterState.Idle);
    }

    protected bool HasMovementInput => facade.HasMovementInput;
    protected float CurrentSpeed => facade.CurrentSpeed;
    protected bool CanJump => facade.CanJump;
    protected bool IsInAir => facade.IsInAir;
    protected bool IsSliding => facade.IsSliding;
    protected bool IsAttacking => facade.IsAttacking;
    protected bool IsBlocking => facade.IsBlocking;

    public class Driver
    {
        public StateEvent Tick;
        public StateEvent FixedTick;
    }
    private void HandleCharacterHit(GameObject hitCharacter, int hitStop)
    {
        if (hitCharacter == gameObject)
        {
            if (!IsBlocking)
            {
                FighterSM.ChangeState(FighterState.HitStun, StateTransition.Safe);
                StartCoroutine(HitStunCoroutine(hitStop));
            }else
            {
                animator.SetTrigger("BlockHit");
            }

        }
    }

    private IEnumerator HitStunCoroutine(int duration)
    {
        for(int i = 0; i < duration; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        FighterSM.ChangeState(FighterState.Idle, StateTransition.Safe);
    }

    private void OnDestroy()
    {
        HitEventManager.OnCharacterHit -= HandleCharacterHit;
    }

    public void Tick()
    {
        FighterSM.Driver.Tick.Invoke();
    }
    public void FixedTick()
    {
        FighterSM.Driver.FixedTick.Invoke();
    }

    public void Idle_Enter()
    {
        Debug.Log("Entering Idle");
        animator.SetFloat("Speed", 0f);
    }

    public void Idle_Tick()
    {
        // If there is movement input, switch to Walking.
        if (HasMovementInput && !IsInAir)
        {
            FighterSM.ChangeState(FighterState.Walking, StateTransition.Safe);
        }
        // If jump is pressed and we can jump, switch to Jumping.
        if (inputModule.IsKeyPressed("Jump") && CanJump)
        {
            FighterSM.ChangeState(FighterState.Jumping, StateTransition.Safe);
        }

        if (IsAttacking)
        {
            FighterSM.ChangeState(FighterState.Attacking, StateTransition.Safe); ;
        }

        if (IsBlocking)
        {
            FighterSM.ChangeState(FighterState.Blocking, StateTransition.Safe);
        }
    }

    public void Walking_Enter()
    {
        Debug.Log("FSM: Entering Walking");

        float targetSpeed = CurrentSpeed < minWalkSpeed ? 0f : (CurrentSpeed < stopThreshold ? .5f : 1f);

        animator.SetFloat("Speed", targetSpeed, 0.1f, Time.deltaTime);
    }

    public void Walking_Tick()
    {
        // If no movement input, return to Idle.
        if (!HasMovementInput && !IsSliding)
        {
            FighterSM.ChangeState(FighterState.Idle, StateTransition.Safe);
        }
        else if (!HasMovementInput && IsSliding)
        {
            FighterSM.ChangeState(FighterState.Sliding, StateTransition.Safe);
        }
        if (IsAttacking)
        {
            FighterSM.ChangeState(FighterState.Attacking, StateTransition.Safe); ;
        }
        // If jump is pressed while walking, transition to Jumping.
        if (inputModule.IsKeyPressed("Jump") && CanJump)
        {
            FighterSM.ChangeState(FighterState.Jumping, StateTransition.Safe);
        }
        if (IsBlocking)
        {
            FighterSM.ChangeState(FighterState.Blocking, StateTransition.Safe);
        }
        float targetSpeed = CurrentSpeed < minWalkSpeed ? 0f : (CurrentSpeed < stopThreshold ? 0.5f : 1f);
        animator.SetFloat("Speed", targetSpeed, 0.1f, Time.deltaTime);
    }

    public void Walking_Exit()
    {

    }

    public void Sliding_Enter()
    {
        Debug.Log("FSM: Entering Sliding");
        animator.SetTrigger("Slide");
    }

    public void Sliding_Tick()
    {
        if (!IsSliding)
        {
            FighterSM.ChangeState(FighterState.Idle, StateTransition.Safe);
        }
    }

    public void Jumping_Enter()
    {
        AudioManager.Instance.PlaySound("Jump");
        Debug.Log("FSM: Entering Jumping");
        // Trigger jump animation immediately.
        animator.SetTrigger("Jump");
        animator.SetFloat("Speed", 0);
    }

    public void Jumping_Tick()
    {
        if (!IsInAir)
        {
            FighterSM.ChangeState(FighterState.Idle, StateTransition.Safe);
        }
        animator.SetFloat("Speed", 0);
    }

    public void Jumping_Exit()
    {
        Debug.Log("FSM: Exiting Jumping");
    }

    public void Attacking_Enter()
    {
        Debug.Log("FSM: Entering Attacking");
    }

    public void Attacking_Tick()
    {
        // While attacking, the FighterAttackModule manages hitbox activation and timing.
        // Here we check if the attack is complete.
        if (!IsAttacking)
        {
            // Transition back to Idle (or to another state based on input).
            FighterSM.ChangeState(FighterState.Idle, StateTransition.Safe);
        }
    }

    public void Attacking_Exit()
    {
        Debug.Log("FSM: Exiting Attacking");
    }

    public void HitStun_Enter()
    {
        Debug.Log("FSM: Entering HitStun");
        AudioManager.Instance.PlaySound("Hurt");
        // You might also set an animator bool here, e.g.:
        animator.SetBool("IsHitStunned", true);
    }

    public void HitStun_Tick() { } // Optionally do something per frame if needed

    public void HitStun_Exit()
    {
        animator.SetBool("IsHitStunned", false);
        Debug.Log("FSM: Exiting HitStun");
    }

    public void Blocking_Enter()
    {
        Debug.Log("FSM: Entering Blocking");
        animator.SetBool("Block", true);
    }

    public void Blocking_Tick()
    {
        if (!IsBlocking)
        {
            FighterSM.ChangeState(FighterState.Idle, StateTransition.Safe);
        }
    }

    public void Blocking_Exit()
    {
        Debug.Log("FSM: Exiting Blocking");

        animator.SetBool("Block", false);
    }
}
