using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveSlot
{
    public AttackCategory category;
    public string moveName; // e.g., "Light Attack 1"
    public AttackData_SO attackData; // Or SpecialMoveData if needed.
}

[CreateAssetMenu(menuName = "Attack/Move Profile List")]
public class MoveProfile_SO : ScriptableObject
{
    public List<MoveSlot> moves;

    public List<ComboAttackData_SO> comboAttacks;
}

