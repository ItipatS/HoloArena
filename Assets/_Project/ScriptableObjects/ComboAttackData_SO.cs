using UnityEngine;

[CreateAssetMenu(menuName = "Attack/Combo Attack Data")]
public class ComboAttackData_SO : ScriptableObject
{
    [Header("Combo Sequence")]
    public AttackData_SO[] comboMoves;  // E.g., Jab1, Jab2, Jab3

    [Header("Combo Settings")]
    public float comboWindow;        // How long the player has to input the next attack
    // You can add additional combo-specific fields like combo multiplier, etc.
}
