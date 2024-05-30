using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TromboneBuild))]
[CanEditMultipleObjects]
public class TromboneBuildInspector : Editor
{
    private TromboneBuild targetBuild;

    private void OnEnable()
    {
        if (targets.Length == 1)
            targetBuild = target as TromboneBuild;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (targetBuild == null) return;
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy to scene"))
        {
            Undo.RecordObject(targetBuild, "TromboneBuild to Scene"); 
            targetBuild.SetBuildToScene();
            EditorApplication.QueuePlayerLoopUpdate();
        }
        if (GUILayout.Button("Copy from scene"))
        {
            Undo.RecordObject(targetBuild, "Scene to TromboneBuild");
            targetBuild.GetBuildFromScene();
            EditorApplication.QueuePlayerLoopUpdate();
        }
        //GUI.enabled = (targetBuild != TromboneBuild.RuntimeBuild);
        //if (GUILayout.Button("Copy to runtime"))
        //{
        //    Undo.RecordObject(targetBuild, "TromboneBuild to Runtime");
        //    TromboneBuild.Copy(targetBuild, TromboneBuild.RuntimeBuild);
        //    EditorApplication.QueuePlayerLoopUpdate();
        //}
        //if (GUILayout.Button("Copy from runtime"))
        //{
        //    Undo.RecordObject(targetBuild, "Runtime to TromboneBuild");
        //    TromboneBuild.Copy(TromboneBuild.RuntimeBuild, targetBuild);
        //    EditorApplication.QueuePlayerLoopUpdate();
        //}
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
