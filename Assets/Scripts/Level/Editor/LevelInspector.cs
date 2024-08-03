using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Level))]
public class LevelInspector : Editor
{
    private Level level;

    public static ObjectiveInfo[] objectiveClipboard;

    private void OnEnable()
    {
        level = target as Level;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Copy objectives")) CopyObjectives();
        bool enableGUI = GUI.enabled;
        GUI.enabled = objectiveClipboard != null;
        if (GUILayout.Button("Paste objectives")) PasteObjectives();
        GUI.enabled = enableGUI;
        EditorGUILayout.EndHorizontal();
    }

    private void CopyObjectives()
    {
        int objectiveCount = level.objectives != null ? level.objectives.Length : 0;
        objectiveClipboard = new ObjectiveInfo[objectiveCount];
        if (objectiveCount > 0) Array.Copy(level.objectives, objectiveClipboard, objectiveCount);
    }

    private void PasteObjectives()
    {
        Undo.RecordObject(level, "Paste objectives");
        int objectiveCount = objectiveClipboard != null ? objectiveClipboard.Length : 0;
        level.objectives = new ObjectiveInfo[objectiveCount];
        if (objectiveCount > 0) Array.Copy(objectiveClipboard, level.objectives, objectiveCount);
        EditorUtility.SetDirty(level);
    }
}
