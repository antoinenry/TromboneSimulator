using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MusicPlayer))]
public class MusicPlayerInspector : Editor
{
    private MusicPlayer musicPlayer;
    private bool transitionEffects;

    private void OnEnable()
    {
        musicPlayer = target as MusicPlayer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Control Panel", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        bool GUIEnabled = GUI.enabled;
        GUI.enabled = (musicPlayer.State == MusicPlayer.PlayState.Play) ? false : GUIEnabled;
        if (GUILayout.Button("Play")) musicPlayer.Play(transitionEffects);
        GUI.enabled = (musicPlayer.State != MusicPlayer.PlayState.Play) ? false : GUIEnabled;
        if (GUILayout.Button("Pause")) musicPlayer.Pause(transitionEffects);
        GUI.enabled = (musicPlayer.State == MusicPlayer.PlayState.Stop) ? false : GUIEnabled;
        if (GUILayout.Button("Stop")) musicPlayer.Stop(transitionEffects);
        GUI.enabled = GUIEnabled;
        EditorGUILayout.EndHorizontal();
        transitionEffects = EditorGUILayout.Toggle("Transition effects", transitionEffects);
        EditorGUILayout.LabelField("Playing state", musicPlayer.State.ToString());
        EditorGUILayout.LabelField("Playing speed", musicPlayer.CurrentPlayingSpeed.ToString());
        EditorGUILayout.EndVertical();
    }
}
