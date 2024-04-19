using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelSelectionScreen))]
public class LevelSelectionScreenInspector : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            (target as LevelSelectionScreen).UpdateLevelList();
        }
    }
}