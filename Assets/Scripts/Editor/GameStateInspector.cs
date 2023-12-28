using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameState))]
public class GameStateInspector : Editor
{
    GameState state;

    private void OnEnable()
    {
        state = target as GameState;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Reset scores"))
        {
            state.ClearCurrentScore();
            state.ClearHighscores();
        }
        if (GUILayout.Button("Save"))
        {
            state.SaveState();
        }
        if (GUILayout.Button("Load"))
        {
            state.TryLoadState();
        }
        EditorGUILayout.EndHorizontal();
    }
}
