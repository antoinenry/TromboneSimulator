using UnityEngine;
using System;

[Serializable]
public class NotePlacement
{
    public string instrumentName = "Trombone";
    public NoteGridDimensions grid = NoteGridDimensions.DefaultTromboneGrid;
    public Vector2[] noteCoordinates;

    public Vector2 GetPlacement(INote note)
    {
        return new Vector2(float.NaN, float.NaN);
    }

    public Vector2 GetPlacement(int noteIndex)
    {
        int coordinateCount = noteCoordinates != null ? noteCoordinates.Length : 0;
        return (noteIndex >= 0 && noteIndex < coordinateCount) ? noteCoordinates[noteIndex] : new Vector2(float.NaN, float.NaN);
    }

    public Vector2[] GetPlacement(int[] noteIndices)
    {
        int noteCount = noteIndices == null ? noteIndices.Length : 0;
        Vector2[] coordinates = new Vector2[noteCount];
        for (int n = 0, nCount = noteIndices.Length; n < nCount; n++)
            coordinates[n] = GetPlacement(noteIndices[n]);
        return coordinates;
    }
}