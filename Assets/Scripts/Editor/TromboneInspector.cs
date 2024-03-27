using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TromboneCore))]
public class TromboneInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TromboneCore trombone = target as TromboneCore;
        EditorGUILayout.LabelField("Current build");
        EditorGUILayout.BeginHorizontal("box");
        EditorGUI.BeginChangeCheck();
        trombone.currentBuild = EditorGUILayout.ObjectField(trombone.currentBuild, typeof(TromboneBuild), false) as TromboneBuild;
        if (EditorGUI.EndChangeCheck() || GUILayout.Button("Recall"))
        {
            trombone.LoadBuild();
            NoteGrid grid = FindObjectOfType<NoteGrid>(true);
            if (grid != null) grid.UpdateGrid();
        }
        if (GUILayout.Button("Save"))
            (target as TromboneCore).SaveBuild();
        EditorGUILayout.EndHorizontal();
    }
}