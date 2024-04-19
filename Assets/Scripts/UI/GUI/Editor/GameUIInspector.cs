using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameUI), true)]
public class GameUIInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Visibility button
        GameUI targetGUI = target as GameUI;
        if (!targetGUI.GUIActive && GUILayout.Button("Show UI")) (target as GameUI).GUIActive = true;
        if (targetGUI.GUIActive && GUILayout.Button("Hide UI")) (target as GameUI).GUIActive = false;
    }
}