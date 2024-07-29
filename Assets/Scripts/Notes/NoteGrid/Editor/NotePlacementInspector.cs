using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NotePlacement))]
public class NotePlacementInspector : Editor
{
    private NotePlacement notePlacement;
    private SheetMusic selectedMusic;
    private int selectedPartIndex;
    private NoteSpawn[] editorNoteSpawns;


    private void OnEnable()
    {
        notePlacement = (NotePlacement)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Edit dimensions", EditorStyles.boldLabel);
        if (GUILayout.Button("Set grid from scene"))
        {
            Undo.RecordObject(notePlacement, "Set Grid From Scene");
            notePlacement.FindCurrentGridDimensions();
            EditorUtility.SetDirty(notePlacement);
        }
        EditorGUILayout.LabelField("Edit coordinates", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set from"))
        {
            Undo.RecordObject(notePlacement, "Set Default Coordinates");
            NoteInfo[] getNotes = selectedMusic?.GetPartNotes(selectedPartIndex);
            notePlacement.SetDefaultCoordinates(getNotes);
            EditorUtility.SetDirty(notePlacement);
        }
        selectedMusic = (SheetMusic)EditorGUILayout.ObjectField(selectedMusic, typeof(SheetMusic), false);
        selectedPartIndex = EditorGUILayout.Popup(selectedPartIndex, selectedMusic != null ? selectedMusic.PartNames : new string[0]);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
