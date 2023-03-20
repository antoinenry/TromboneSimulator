using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameState))]
public class GameStateInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Reset scores"))
        {
            GameState state = target as GameState;
            state.ClearCurrentScore();
            state.ClearHighscores();
        }
        if (GUILayout.Button("Save"))
        {
            GameState state = target as GameState;
            state.SaveState();
        }
        if (GUILayout.Button("Load"))
        {
            GameState state = target as GameState;
            state.TryLoadState();
        }
        EditorGUILayout.EndHorizontal();
    }
}
