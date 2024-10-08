﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditor))]
public class LevelEditorInspector : Editor
{
    private LevelEditor levelEditor;

    private void OnEnable()
    {
        levelEditor = target as LevelEditor;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Level"))
        {
            levelEditor.LoadLevel();
        }
        GUILayout.Button("Save changes");
        {
            if (levelEditor.levelAsset) EditorUtility.SetDirty(levelEditor.levelAsset);
            if (levelEditor.levelAsset?.music) EditorUtility.SetDirty(levelEditor.levelAsset.music);
            if (levelEditor.NotePlacementAsset) EditorUtility.SetDirty(levelEditor.NotePlacementAsset);
        }
        EditorGUILayout.EndHorizontal();
    }
}