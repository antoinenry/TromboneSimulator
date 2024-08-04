using UnityEngine;
using System;
using System.Collections;

public class LevelEditor : MonoBehaviour
{
    [Header("Level")]
    public Level levelAsset;
    public string partName = "Trombone";
    public NotePlacement NotePlacementAsset;
    [Header("Playing")]
    public Playhead editorPlayhead;
    public int currentNoteIndex;
    public NoteInfo currentNoteInfo;
    public float fastSpeed = 2f;
    [Header("Controls")]
    public KeyCode playPauseKey = KeyCode.Space;
    public KeyCode scrollForwardKey = KeyCode.RightArrow;
    public KeyCode scrollBackwardKey = KeyCode.LeftArrow;
    public KeyCode nextPlacementKey = KeyCode.UpArrow;
    public KeyCode previousPlacementKey = KeyCode.DownArrow;
    public KeyCode legatoKey = KeyCode.L;
    public KeyCode reduceDurationKey = KeyCode.Minus;
    public KeyCode augmentDurationKey = KeyCode.Plus;
    public float durationIncrement = .1f;

    private NoteGrid noteGrid;
    private NoteSpawner noteSpawner;
    private MusicPlayer musicPlayer;
    private TromboneCore trombone;

    private void Awake()
    {
        noteGrid = FindObjectOfType<NoteGrid>();
        noteSpawner = FindObjectOfType<NoteSpawner>();
        musicPlayer = FindObjectOfType<MusicPlayer>();
        trombone = FindObjectOfType<TromboneCore>();
    }

    private void OnEnable()
    {        
        editorPlayhead.onStartEnterNote.AddListener(OnPlayheadEntersNote);
    }

    private void OnDisable()
    {
        editorPlayhead.onStartEnterNote.RemoveListener(OnPlayheadEntersNote);
    }

    private void Update()
    {
        if (musicPlayer)
        {
            if (musicPlayer.PlayingState == MusicPlayer.PlayState.Play)
            {
                if (Input.GetKeyDown(playPauseKey)) musicPlayer.Pause();
                else if (Input.GetKey(scrollForwardKey)) musicPlayer.playingSpeed = fastSpeed;
                else if (Input.GetKey(scrollBackwardKey)) musicPlayer.playingSpeed = -fastSpeed;
                else musicPlayer.playingSpeed = 1f;
            }
            else
            {
                if (Input.GetKeyDown(playPauseKey)) musicPlayer.Play();
                else if (Input.GetKey(scrollForwardKey)) musicPlayer.playTime += Time.deltaTime;
                else if (Input.GetKey(scrollBackwardKey)) musicPlayer.playTime -= Time.deltaTime;
            }
        }
        if (currentNoteIndex != -1)
        {
            if (Input.GetKeyDown(nextPlacementKey)) ChangeNotePlacement(currentNoteIndex, true);
            else if (Input.GetKeyDown(previousPlacementKey)) ChangeNotePlacement(currentNoteIndex, false);
            else if (Input.GetKeyDown(augmentDurationKey)) ChangeNoteDurationBy(currentNoteIndex, durationIncrement);
            else if (Input.GetKeyDown(reduceDurationKey)) ChangeNoteDurationBy(currentNoteIndex, -durationIncrement);
            else if (Input.GetKeyDown(legatoKey)) ForceNoteLegato(currentNoteIndex);
        }
    }

    public void LoadLevel()
    {
        StartCoroutine(LoadLevelCoroutine());
    }

    private IEnumerator LoadLevelCoroutine()
    {
        if (musicPlayer != null)
        {
            musicPlayer.LoadMusic(levelAsset?.music, playedInstrument: trombone.Sampler);
            Debug.Log("Loading " + levelAsset?.music?.name);
            yield return new WaitWhile(() => musicPlayer.IsLoading);
            Debug.Log("...complete.");
        }
        if (noteSpawner != null)
        {
            NoteInstance[] notes = musicPlayer?.LoadedNotes;
            Debug.Log("Spawning notes from " + levelAsset?.music?.name);
            noteSpawner.spawnDistance = float.PositiveInfinity;
            noteSpawner.destroyDistance = float.PositiveInfinity;
            noteSpawner.notePlacement = levelAsset?.GetNoteCoordinates(noteSpawner.GridDimensions);
            if (levelAsset) noteSpawner.SpawnNotes(notes, 0f, levelAsset.MusicDuration);
            noteSpawner.time = 0f;
            noteSpawner.UpdateNoteInstances();
            NotePlacementAsset = levelAsset?.GetNotePlacement(noteSpawner.GridDimensions);
        }
    }

    private void OnPlayheadEntersNote(int noteIndex, INote note)
    {
        currentNoteIndex = noteIndex;
        currentNoteInfo = note != null ? NoteInfo.GetInfo(note) : NoteInfo.None;
    }

    private NoteInstance GetNoteInstance(int noteIndex)
    {
        int loadedNoteCount = musicPlayer?.LoadedNotes != null ? musicPlayer.LoadedNotes.Length : 0;
        if (loadedNoteCount <= 0 || noteIndex > loadedNoteCount)
        {
            Debug.LogWarning("Note index out of bound: " + noteIndex + "/" + loadedNoteCount);
            return null;
        }
        INote currentNote = musicPlayer.LoadedNotes[noteIndex];
        if (currentNote == null || currentNote is NoteInstance == false)
        {
            Debug.LogWarning("Couldn't find note instance " + noteIndex + "/" + loadedNoteCount);
            return null;
        }
        return currentNote as NoteInstance;
    }

    private void ChangeNotePlacement(int noteIndex, bool direction)
    {
        int loadedNoteCount = musicPlayer?.LoadedNotes != null ? musicPlayer.LoadedNotes.Length : 0;
        NoteInstance currentNote = GetNoteInstance(noteIndex);
        NoteGridDimensions gridInfo = new();
        if (noteGrid == null)
        {
            Debug.LogWarning("Couldn't find note grid.");
            return;
        }
        if (noteGrid) gridInfo = noteGrid.dimensions;
        Vector2[] spawnedNotesCoordinates = noteSpawner?.notePlacement;
        if (spawnedNotesCoordinates == null)
        {
            Debug.LogWarning("Couldn't find current note placement.");
            return;
        }
        Vector2[] possiblePlacement = gridInfo.ToneToCoordinates(currentNote.Tone);
        if (possiblePlacement == null || possiblePlacement.Length == 0)
        {
            Debug.LogWarning("Couldn't place note " + noteIndex + " on grid");
            return;
        }
        Vector2 currentPlacement = spawnedNotesCoordinates[noteIndex];
        int placementIndex = Array.IndexOf(possiblePlacement, currentPlacement);
        placementIndex = (placementIndex + (direction ? 1 : -1)) % possiblePlacement.Length;
        Vector2 newPlacement = possiblePlacement[placementIndex];
        Debug.Log("Moving note " + currentNoteInfo + " from " + currentPlacement + " to " + newPlacement);
        spawnedNotesCoordinates[noteIndex] = newPlacement;
        NoteSpawn spawn = noteSpawner?.GetSpawn(currentNoteInfo);
        if (spawn) spawn.transform.localPosition = noteGrid.CoordinatesToLocalPosition(newPlacement);
        if (NotePlacementAsset)
        {
            int assetNotePlacementCount = NotePlacementAsset.noteCoordinates != null ? NotePlacementAsset.noteCoordinates.Length : 0;
            if (assetNotePlacementCount != loadedNoteCount) NotePlacementAsset.noteCoordinates = new Vector2[loadedNoteCount];
            Array.Copy(spawnedNotesCoordinates, NotePlacementAsset.noteCoordinates, NotePlacementAsset.noteCoordinates.Length);
        }
    }

    private void SetNoteDuration(int noteIndex, float duration)
    {
        // Edit sheet music
        SheetMusic loadedMusic = levelAsset?.music;
        if (loadedMusic != null)
        {
            NoteInfo noteInfo = loadedMusic.GetNote(partName, noteIndex);
            noteInfo.duration = duration;
            loadedMusic.SetNote(partName, noteIndex, noteInfo);
            Debug.Log("Change note duration :" + noteInfo + ". New duration: " + duration);
        }
        // Show changes on loaded level
        NoteInstance noteInstance = GetNoteInstance(noteIndex);
        if (noteInstance != null)
        {
            NoteSpawn spawn = noteSpawner?.GetSpawn(noteIndex);
            spawn?.SetLength(duration, noteSpawner.TimeScale);
            noteInstance.Duration = duration;
        }
    }

    private void ChangeNoteDurationBy(int noteIndex, float durationDelta)
    {
        NoteInstance noteInstance = GetNoteInstance(noteIndex);
        if (noteInstance == null) return;
        float noteDuration = noteInstance.Duration;
        SetNoteDuration(noteIndex, noteDuration + durationDelta);
    }

    private void ForceNoteLegato(int noteIndex)
    {
        NoteInstance noteInstance = GetNoteInstance(noteIndex);
        NoteInstance nextNoteInstance = GetNoteInstance(noteIndex + 1);
        if (noteInstance == null || nextNoteInstance == null) return;
        SetNoteDuration(noteIndex, nextNoteInstance.StartTime - noteInstance.StartTime);
    }
}
