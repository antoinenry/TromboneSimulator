using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public bool showDebug;
    [Header("Timing")]
    public Playhead playHead;
    public float time;
    public bool reverse;
    [Header("Spawning")]
    public float spawnDistance;
    public float destroyDistance;
    public int yMin = 0;
    public int yMax = 10;
    public Vector2[] notePlacement;
    [Header("Note aspect")]
    public NoteSpawn notePrefab;
    public Color[] colorWheel = new Color[] { Color.blue, Color.red, Color.green, Color.cyan, Color.yellow, Color.magenta };
    public int colorIndex;
    public float incomingTime;
    public float minimumIncomingTime;
    [Header("Events")]
    public UnityEvent<NoteSpawn> onSpawnNote;
    public UnityEvent<NoteSpawn> onDestroyNote;
    public UnityEvent<NoteSpawn[], float, float> onMoveNotes;

    private NoteGrid grid;
    private List<NoteSpawn> noteSpawns;
    private float previousTime;

    public float TimeScale => grid != null ? grid.timeScale : 0f;
    public float SpawnDelay
    {
        get
        {
            if (TimeScale == 0f) return 0f;
            if (reverse == false) return spawnDistance / TimeScale;
            else return -destroyDistance / TimeScale;
        }
    }
    public NoteGridDimensions GridDimensions => grid != null ? grid.dimensions : new NoteGridDimensions();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + destroyDistance * Vector3.left, transform.position + spawnDistance * Vector3.right);
        Gizmos.DrawLine(transform.position + destroyDistance * Vector3.down, transform.position + spawnDistance * Vector3.up);
    }

    private void Awake()
    {
        grid = GetComponent<NoteGrid>();
        ClearNotes();
    }

    private void OnEnable()
    {
        if (playHead != null)
        {
            playHead.onMove.AddListener(OnPlayheadMove);
            playHead.onStartEnterNote.AddListener(OnPlayheadEntersNote);
            playHead.onStop.AddListener(ClearNotes);
        }
        if (grid?.trombone?.tromboneDisplay != null)
        {
            yMin = (int)grid.trombone.tromboneDisplay.minPressureLevel;
            yMax = (int)grid.trombone.tromboneDisplay.maxPressureLevel;
        }
    }

    private void OnDisable()
    {
        if (playHead != null)
        {
            playHead.onMove.RemoveListener(OnPlayheadMove);
            playHead.onStartEnterNote.RemoveListener(OnPlayheadEntersNote);
            playHead.onStop.RemoveListener(ClearNotes);
        }
        ClearNotes();
    }

    private void Update()
    {
        // Adjust playhead offset to spawn distance
        if (playHead != null) playHead.timeOffset = SpawnDelay;
        // Without playhead, update notes on application time
        else
        {
            previousTime = time;
            time += Time.deltaTime;
            UpdateNoteInstances();
        }
    }

    public void UpdateNoteInstances()
    {
        NoteSpawn previousNoteInstance = null;
        foreach (NoteSpawn instance in noteSpawns)
        {
            if (instance != null)
            {
                NoteInfo note = NoteInfo.GetInfo(instance);
                // Destroy note
                if (IsOutOfTimeBounds(note))
                {
                    if (showDebug)
                    {
                        GetNoteDimensions(note, out float noteStartDistance, out float noteLength);
                        Debug.Log("Destroying " + instance);
                        Debug.Log("Out of bound at " + noteStartDistance + ", lenght " + noteLength);
                    }
                    DestroyNote(instance);
                }
                // Or move note
                else
                {
                    instance.Move(time, TimeScale);
                    // Flat mode
                    instance.SetVisible(!grid.flattenY, !grid.flattenX);
                }
                // Link/Unlink notes
                previousNoteInstance?.TryLinkToNextNote(instance);
                previousNoteInstance = instance;
            }
        }
        // Forget destroyed notes
        noteSpawns.RemoveAll(n => n == null);
        // Signal note movement
        onMoveNotes.Invoke(noteSpawns.ToArray(), previousTime, time);
    }

    private void OnPlayheadMove(float from, float to)
    {
        // Update time
        previousTime = from;
        time = to;
        // Direction
        float deltaPlayHead = to - from;
        if (deltaPlayHead > 0f) reverse = false;
        else if (deltaPlayHead < 0f) reverse = true;
        //// Update notes
        UpdateNoteInstances();
    }    

    public void ClearNotes()
    {
        // Destroy note instances
        if (noteSpawns != null)
            foreach (NoteSpawn n in noteSpawns)
                DestroyNote(n);
        noteSpawns = new List<NoteSpawn>();
    }

    private void OnPlayheadEntersNote(int noteIndex, INote note)
    {
        if (note != null) SpawnNote(note, noteIndex);
    }

    public NoteSpawn SpawnNote(INote note, int index)
    {
        if (note == null) return null;
        NoteInfo noteInfo = NoteInfo.GetInfo(note);
        if (showDebug) Debug.Log("Spawning " + noteInfo);
        NoteSpawn spawnedNote = null;
        if (noteInfo.duration > 0f)
        {
            // Don't spawn if out of bounds
            if (IsOutOfTimeBounds(noteInfo))
            {
                if (showDebug)
                {
                    GetNoteDimensions(noteInfo, out float noteStartDistance, out float noteLength);
                    Debug.LogWarning("-> trying to spawn out of bound at " + noteStartDistance + ", lenght " + noteLength);
                }
            }
            else
            // Spawn note
            {
                // Get note position on grid
                Vector2 noteCoordinate = GetNotePlacement(note, index);
                // Spawn note if position is valid
                if (!float.IsNaN(noteCoordinate.x) && !float.IsNaN(noteCoordinate.y) && grid.dimensions.Contains(noteCoordinate, yMin, yMax))
                {
                    spawnedNote = Instantiate(notePrefab, transform);
                    spawnedNote.name = "Note " + index + " (" + ToneAttribute.GetNoteName(note.Tone) + ")";
                    //bool linkToPreviousNote = noteInfo.previousTone != -1 && grid.ToneToCoordinates(noteInfo.previousTone).y == grid.ToneToCoordinates(noteInfo.tone).y;
                    //bool linkToNextNote = noteInfo.nextTone != -1 && grid.ToneToCoordinates(noteInfo.nextTone).y == grid.ToneToCoordinates(noteInfo.tone).y;
                    spawnedNote.transform.localPosition = grid.CoordinatesToLocalPosition(noteCoordinate);
                    NoteInfo.SetInfo(spawnedNote, noteInfo);
                    while (incomingTime < minimumIncomingTime) incomingTime *= 2f;
                    spawnedNote.Init(time, TimeScale, colorWheel[(colorIndex++) % colorWheel.Length], incomingTime); //, linkToPreviousNote, linkToNextNote);
                    noteSpawns.Add(spawnedNote);
                    onSpawnNote.Invoke(spawnedNote);
                    if (showDebug) Debug.Log("-> spawned " + spawnedNote);
                }
                // The grid's dimensions doesn't allow the note to be placed on it
                else if (showDebug) Debug.LogWarning("-> can't place " + noteInfo + " on grid. Spawn is cancelled.");         
            }
        }
        return spawnedNote;
    }

    public void SpawnNotes(INote[] notes, float fromTime, float toTime, int startIndex = 0)
    {
        int noteCount = notes != null ? notes.Length : 0;
        if (noteCount == 0) return;
        // Move playhead without triggering events
        Playhead.ProgressOnNote[] progressOnNotes = playHead.Move(notes, fromTime, toTime, true, true, false);
        // Spawn all notes detected by playhead
        for (int n = 0; n < noteCount; n++)
            if (progressOnNotes[n] != Playhead.ProgressOnNote.None)
                SpawnNote(notes[n], startIndex + n);
        // Update notes
        UpdateNoteInstances();
    }

    public Vector2 GetNotePlacement(INote note, int index)
    {
        Vector2[] possibleCoordinates = grid.dimensions.ToneToCoordinates(note.Tone);
        // Exception 1: no possible coordinates, return undefined value
        int possibleCoordinatesCount = possibleCoordinates != null ? possibleCoordinates.Length : 0;
        if (possibleCoordinatesCount == 0)
        {
            if (showDebug) Debug.LogWarning("No possible placement for note " + note.Tone);
            return new Vector2(float.NaN, float.NaN);
        }
        // Exception 2: no predefined placement, return a default value
        int predefinedPlacementCount = notePlacement != null ? notePlacement.Length : 0;
        if (index < 0 || index >= predefinedPlacementCount) return possibleCoordinates[0];
        // Get predefined placement
        Vector2 predefined = notePlacement[index];
        // Exception 3: predefined placement doesn't match the note and grid, return a default value
        if (note == null || grid == null || Array.IndexOf(possibleCoordinates, predefined) == -1)
        {
            if (showDebug) Debug.LogWarning("Placement mismatch for note " + note.Tone + ". Using default placement " + possibleCoordinates[0]);
            return possibleCoordinates[0];
        }
        // Ok to use predefined value
        return predefined;
    }

    public NoteSpawn GetSpawn(NoteInfo noteInfo) => noteSpawns?.Find(n => n != null && n.noteInfo == noteInfo);
    public NoteSpawn GetSpawn(int index)
    {
        int spawnCount = noteSpawns != null ? noteSpawns.Count : 0;
        return index < 0 || index > spawnCount ? null : noteSpawns[index];
    }

    private void DestroyNote(NoteSpawn note)
    {
        if (note != null)
        {
            Destroy(note.gameObject);
            onDestroyNote.Invoke(note);
        }
    }

    private void GetNoteDimensions(NoteInfo noteInfo, out float noteStartDistance, out float noteLength)
    {
        noteStartDistance = (noteInfo.startTime - time) * TimeScale;
        noteLength = noteInfo.duration * TimeScale;
    }

    private bool IsOutOfTimeBounds(NoteInfo noteInfo)
    {
        GetNoteDimensions(noteInfo, out float noteStartDistance, out float noteLength);
        return IsOutOfTimeBounds(noteStartDistance, noteLength);
    }

    private bool IsOutOfTimeBounds(float noteStartDistance, float noteLength)
    {
        if (Mathf.Approximately(noteStartDistance, spawnDistance)) return false;
        if (Mathf.Approximately(noteStartDistance + noteLength, -destroyDistance)) return false;
        return noteStartDistance > spawnDistance || noteStartDistance + noteLength < -destroyDistance;
    }
}
