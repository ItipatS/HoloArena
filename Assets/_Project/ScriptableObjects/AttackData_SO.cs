using UnityEngine;

public enum AttackCategory 
{
    Light,
    Heavy,
    Special,
    Combo
}
public enum AttackInput
{
    LightAttack1,
    LightAttack2,
    HeavyAttack1,
    HeavyAttack2
}

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    [Header("General Info")]
    public string attackName;         // e.g., "Jab", "LowKick"
    public AttackCategory category;
    public float stamina;

    [Header("Special Combo Settings")]
    // For special moves, define the required input sequence
    // (For normal moves, you can leave this empty)
    public AttackInput[] specialInput;
    public float specialComboMaxGap;     // Maximum gap allowed between inputs (in frames or seconds)

    [Header("Air Attack Settings")]
    public bool AirAttack = false;

    [Header("Input Configuration")]
    // Instead of a free-form string, use dropdowns via custom editor:
    public AttackInput inputName;        // The key or sequence trigger for this move

    [Header("Timing")]
    public int startupFrames; // time before hitbox is active
    public int activeFrames;  // time hitbox is active
    public int recoveryFrames; // time after active period

    [Header("Damage & Effects")]
    public int baseDamage;
    public float knockbackMagnitude;    // overall force magnitude
    public float knockbackVerticalFactor; // factor for upward force
    public int hitStop;

    [Header("Cancelability Settings")]
    public bool cancelableDuringStartup = true;
    public bool cancelableDuringRecovery = true;

    [Header("Hitbox Settings")]
    public Vector3 hitboxSize;

    [Header("Animation")]
    public string animationTrigger;   // Animation trigger name in the Animator
}
