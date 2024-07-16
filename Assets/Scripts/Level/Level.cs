using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Trombone Hero/Game Data/Level")]
public class Level : ScriptableObject, IUnlockableContent
{
    public SheetMusic music;
    public int unlockTier;
    public NotePlacement[] notePlacements;
    public ObjectiveInfo[] objectives;

    public bool AutoUnlock => true;
    public int UnlockTier => unlockTier;
    public float MusicDuration => music != null ? music.GetDuration() : 0;

    public NotePlacement GetNotePlacement(NoteGridDimensions gridDimensions)
    {
        if (notePlacements == null) return null;
        return Array.Find(notePlacements, p => p != null && p.grid == gridDimensions);
    }

    public Vector2[] GetNoteCoordinates(NoteGridDimensions gridDimensions)
    {
        NotePlacement placement = GetNotePlacement(gridDimensions);
        return placement?.noteCoordinates;
    }
}