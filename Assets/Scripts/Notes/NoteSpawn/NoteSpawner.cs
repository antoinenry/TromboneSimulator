using UnityEngine;
using UnityEngine.Events;
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
    private List<NoteSpawn> noteInstances;
    private NotePlacement notePlacement;
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
        foreach (NoteSpawn instance in noteInstances)
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
                        Debug.Log("Destoying " + instance);
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
        noteInstances.RemoveAll(n => n == null);
        // Signal note movement
        onMoveNotes.Invoke(noteInstances.ToArray(), previousTime, time);
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
        if (noteInstances != null)
            foreach (NoteSpawn n in noteInstances)
                DestroyNote(n);
        noteInstances = new List<NoteSpawn>();
    }

    private void OnPlayheadEntersNote(int noteIndex, INote note)
    {
        if (note != null) SpawnNote(note);
    }

    public NoteSpawn SpawnNote(INote note)
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
                Vector2 noteCoordinate = GetNotePlacement(note);
                // Spawn note if position is valid
                if (!float.IsNaN(noteCoordinate.x) && !float.IsNaN(noteCoordinate.y) && grid.dimensions.Contains(noteCoordinate, yMin, yMax))
                {
                    spawnedNote = Instantiate(notePrefab, transform);
                    spawnedNote.name = ToneAttribute.GetNoteName(note.Tone);
                    //bool linkToPreviousNote = noteInfo.previousTone != -1 && grid.ToneToCoordinates(noteInfo.previousTone).y == grid.ToneToCoordinates(noteInfo.tone).y;
                    //bool linkToNextNote = noteInfo.nextTone != -1 && grid.ToneToCoordinates(noteInfo.nextTone).y == grid.ToneToCoordinates(noteInfo.tone).y;
                    spawnedNote.transform.localPosition = grid.CoordinatesToLocalPosition(noteCoordinate);
                    NoteInfo.SetInfo(spawnedNote, noteInfo);
                    while (incomingTime < minimumIncomingTime) incomingTime *= 2f;
                    spawnedNote.Init(time, TimeScale, colorWheel[(colorIndex++) % colorWheel.Length], incomingTime); //, linkToPreviousNote, linkToNextNote);
                    noteInstances.Add(spawnedNote);
                    onSpawnNote.Invoke(spawnedNote);
                    if (showDebug) Debug.Log("-> spawned " + spawnedNote);
                }
                // The grid's dimensions doesn't allow the note to be placed on it
                else if (showDebug) Debug.LogWarning("-> can't place " + noteInfo + " on grid. Spawn is cancelled.");         
            }
        }
        return spawnedNote;
    }

    public void SpawnNotes(INote[] notes, float fromTime, float toTime)
    {
        int noteCount = notes != null ? notes.Length : 0;
        if (noteCount == 0) return;
        // Remember time
        float keep_previousTime = previousTime;
        float keep_time = time;
        previousTime = fromTime;
        time = toTime;
        // Move playhead without triggering events
        Playhead.ProgressOnNote[] progressOnNotes = playHead.Move(notes, fromTime, toTime, true, true, false);
        // Spawn all notes detected by playhead
        for (int n = 0; n < noteCount; n++)
            if (progressOnNotes[n] != Playhead.ProgressOnNote.None)
                SpawnNote(notes[n]);
        // Restore time
        previousTime = keep_previousTime;
        time = keep_time;
    }

    public Vector2 GetNotePlacement(INote note)
    {
        Vector2 noteCoordinate = new Vector2(float.NaN, float.NaN);
        if (note != null)
        {
            // Get position from custom note placement
            if (notePlacement != null) noteCoordinate = notePlacement.GetPlacement(note);
            // Or get a default position from grid
            else noteCoordinate = grid.dimensions.ToneToCoordinate(note.Tone);
        }
        return noteCoordinate;
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
