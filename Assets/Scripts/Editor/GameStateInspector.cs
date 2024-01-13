using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameState_old))]
public class GameStateInspector : Editor
{
    GameState_old state;

    private void OnEnable()
    {
        state = target as GameState_old;
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
