//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(LevelEditor))]
//public class LevelEditorInspector : Editor
//{
//    private LevelEditor levelEditor;

//    private void OnEnable()
//    {
//        levelEditor = target as LevelEditor;
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        GUIStyle dotButtonStyle = new(EditorStyles.miniButton);
//        dotButtonStyle.stretchWidth = false;
//        EditorGUILayout.LabelField("Resources folders", EditorStyles.boldLabel);

//        EditorGUILayout.BeginHorizontal();
//        levelEditor.musicSheetResourceFolder = EditorGUILayout.TextField("Music sheets location", levelEditor.musicSheetResourceFolder);
//        bool browseFiles = GUILayout.Button("...", dotButtonStyle);
//        EditorGUILayout.EndHorizontal();
//        if (browseFiles) levelEditor.musicSheetResourceFolder = EditorUtility.OpenFolderPanel("MusicSheet Folder", levelEditor.musicSheetResourceFolder, levelEditor.musicSheetResourceFolder);

//        EditorGUILayout.BeginHorizontal();
//        levelEditor.notePlacementResourceFolder = EditorGUILayout.TextField("Note placements location", levelEditor.notePlacementResourceFolder);
//        browseFiles = GUILayout.Button("...", dotButtonStyle);
//        EditorGUILayout.EndHorizontal();
//        if (browseFiles) levelEditor.notePlacementResourceFolder = EditorUtility.OpenFolderPanel("NoteGridPlacements Folder", levelEditor.notePlacementResourceFolder, levelEditor.notePlacementResourceFolder);

//        if (levelEditor.selectedNoteInstance != null)
//        {
//            NoteInfo note = NoteInfo.GetInfo(levelEditor.selectedNoteInstance);
//            EditorGUI.BeginChangeCheck();
//            GUILayout.BeginVertical("box");
//            note.startTime = EditorGUILayout.FloatField("Time", note.startTime);
//            note.duration = EditorGUILayout.FloatField("Duration", note.duration);
//            note.tone = EditorGUILayout.FloatField("Tone", note.tone);
//            GUILayout.BeginHorizontal();
//            EditorGUILayout.Vector2Field("Coordinates", Vector2.zero);
//            if (GUILayout.Button("Change")) levelEditor.AlternateNoteCoordinates();
//            GUILayout.EndHorizontal();
//            GUILayout.EndVertical();
//            if (EditorGUI.EndChangeCheck()) levelEditor.SetNoteInfo(note);
//        }

//        if (Application.isPlaying)
//        {
//            if (GUILayout.Button("Save Level")) EditorUtility.SetDirty(levelEditor.level);
//        }
//    }
//}