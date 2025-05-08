using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AttackData_SO))]
public class AttackDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the target AttackData.
        AttackData_SO attackData = (AttackData_SO)target;

        attackData.category = (AttackCategory)EditorGUILayout.EnumPopup("Category", attackData.category);
        attackData.inputName = (AttackInput)EditorGUILayout.EnumPopup("InputName", attackData.inputName);

        DrawDefaultInspector();
    }
}
