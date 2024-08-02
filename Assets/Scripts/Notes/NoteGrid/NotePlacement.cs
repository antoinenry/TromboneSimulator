using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewNotePlacement", menuName = "Trombone Hero/Note Placement")]
public class NotePlacement : ScriptableObject
{
    public NoteGridDimensions grid = NoteGridDimensions.DefaultTromboneGrid;
    public Vector2[] noteCoordinates;

    private void Reset()
    {
        FindCurrentGridDimensions();
    }

    public void FindCurrentGridDimensions()
    {
        NoteGrid currentNoteGrid = FindObjectOfType<NoteGrid>(true);
        if (currentNoteGrid != null) grid = currentNoteGrid.dimensions;
    }

    public void SetDefaultCoordinates(NoteInfo[] notes)
    {
        int noteCount = notes != null ? notes.Length : 0;
        if (noteCount == 0) noteCoordinates = new Vector2[0];
        else noteCoordinates = Array.ConvertAll(notes, n => grid.ToneToCoordinate(n.tone));
    }
}