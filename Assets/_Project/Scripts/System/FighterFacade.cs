using UnityEngine;

public class FighterFacade : MonoBehaviour, ICharacterModule
{
    public MovementModule MovementModule { get; private set; }
    public GravityComponent GravityComponent { get; private set; }
    public GroundCheckModule GroundCheckModule { get; private set; }
    public FighterAttackModule AttackModule { get; private set; }
    public FighterBlockModule BlockModule { get; private set; }

    // Optionally store a reference to the FighterController if needed.
    private FighterController controller;

    // Called by the FighterController when initializing modules.
    public void Initialize(FighterController controller)
    {
        this.controller = controller;

        // Retrieve modules from the controller's GameObject or its children.
        // Depending on your setup, you might want to use GetModule<T>() to avoid duplicates.
        MovementModule = controller.GetModule<MovementModule>();
        GravityComponent = controller.GetModule<GravityComponent>();
        GroundCheckModule = controller.GetModule<GroundCheckModule>();
        AttackModule = controller.GetModule<FighterAttackModule>();
        BlockModule = controller.GetModule<FighterBlockModule>();
    }

    public void Tick(float deltaTime)
    {
    }

    public void FixedTick(float fixedDeltaTime)
    {
    }
    public bool HasMovementInput => MovementModule != null && MovementModule.HasMovementInput;
    public float CurrentSpeed => MovementModule != null ? MovementModule.CurrentSpeed : 0f;
    public bool CanJump => GroundCheckModule != null && GroundCheckModule.CanJump;
    public bool IsInAir => GroundCheckModule != null && !GroundCheckModule.IsGrounded;
    public bool IsSliding => MovementModule != null && MovementModule.IsSliding;
    public bool IsAttacking => AttackModule != null && AttackModule.IsAttacking;
    public bool IsBlocking => BlockModule != null && BlockModule.IsBlocking;
    
    public bool IsDashing => MovementModule != null && MovementModule.IsDashing;
}
