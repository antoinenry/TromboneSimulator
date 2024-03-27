using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TromboneCore))]
public class TromboneCoreInspector : Editor
{
    private TromboneCore trombone;
    private TromboneBuild build;
    private bool loadBuild;
    private bool saveBuild;

    private void OnEnable()
    {
        trombone = target as TromboneCore;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) trombone.onChangeBuild.Invoke();
        EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (!loadBuild && !saveBuild)
        {
            if (GUILayout.Button("Load")) loadBuild = true;
            if (GUILayout.Button("Save")) saveBuild = true;
        }
        else if (loadBuild)
        {
            build = EditorGUILayout.ObjectField("Load from", build, typeof(TromboneBuild), false) as TromboneBuild;
            if (build != null)
            {
                build.LoadTo(trombone);
                if (saveBuild) build.SaveFrom(trombone);
                if (GUILayout.Button("OK", EditorStyles.miniButtonRight)) loadBuild = false;
            }
            else if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight)) loadBuild = false;
        }
        else if (saveBuild)
        {
            build = EditorGUILayout.ObjectField("Save to", build, typeof(TromboneBuild), false) as TromboneBuild;
            if (build != null && GUILayout.Button("OK", EditorStyles.miniButtonRight))
            {
                build.SaveFrom(trombone);
                saveBuild = false;
            }
            if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight))
                saveBuild = false;
        }
        EditorGUILayout.EndHorizontal();
    }
}