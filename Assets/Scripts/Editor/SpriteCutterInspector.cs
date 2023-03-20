using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteCutter))]
public class SpriteCutterInspector : Editor
{
    private float cutStart;
    private float cutEnd;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Cut"))
            {
                (target as SpriteCutter).Cut(cutStart, cutEnd);
            }
            cutStart = EditorGUILayout.FloatField(cutStart);
            cutEnd = EditorGUILayout.FloatField(cutEnd);
            EditorGUILayout.EndHorizontal();
        }
    }
}
