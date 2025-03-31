using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Trombone Hero/Level")]
public class Level : ScriptableObject, IUnlockableContent
{
    public string levelName;
    public SheetMusic music;
    public int unlockTier;
    public float timeScale = 90f;
    public NotePlacement[] notePlacements;
    public ObjectiveInfo[] objectives;
    public LevelEventSheet[] events;

    public ScriptableObject ContentAsset => this;
    public bool AutoUnlock => true;
    public int UnlockTier
    {
        get => unlockTier;
        set => unlockTier = value;
    }
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