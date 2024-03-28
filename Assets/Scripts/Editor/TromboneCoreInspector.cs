using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TromboneCore))]
public class TromboneCoreInspector : Editor
{
    private TromboneCore targetTrombone;

    private void OnEnable()
    {
        targetTrombone = target as TromboneCore;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        bool enableGUI = GUI.enabled;
        GUI.enabled = targetTrombone.CurrentBuild != null;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Build")) targetTrombone.ApplyBuild();
        if (GUILayout.Button("Save Build")) targetTrombone.SaveCurrentBuild();
        EditorGUILayout.EndHorizontal();
        GUI.enabled = enableGUI;
    }
}