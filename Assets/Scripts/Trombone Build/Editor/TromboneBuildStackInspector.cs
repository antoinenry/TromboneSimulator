using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TromboneBuildStack))]
public class TromboneBuildStackInspector : Editor
{
    private TromboneBuildStack stack;
    private int[] nullModindices;
    private int[][] cantStackModIndices;

    private void OnEnable()
    {
        stack = target as TromboneBuildStack;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (stack.IsStackClean(out nullModindices, out cantStackModIndices) == false)
        {
            if (nullModindices.Length > 0)
            {
                string indicesString = "";
                foreach (int index in nullModindices) indicesString += index + ", ";
                EditorGUILayout.HelpBox("Null mods: " + indicesString, MessageType.Warning);
            }
            if (cantStackModIndices.Length > 0)
            {
                foreach (int[] indexGroup in cantStackModIndices)
                {
                    string indicesString = "";
                    foreach (int index in indexGroup) indicesString += index + ", ";
                    EditorGUILayout.HelpBox("Can't stack " + stack.mods[indexGroup[0]] + " : " + indicesString, MessageType.Warning);
                }
            }
            if (GUILayout.Button("Clean up")) stack.CleanUp();
        }
    }
}