using UnityEngine;
using System;

public class LevelEditor : MonoBehaviour
{
    [Header("Controls")]
    public float scrollDuration = .5f;
    public float zoomSpeed = 1f;
    public float fastPlaySpeed = 4f;
    public float lengthChangeSpeed = .1f;
    [Header("Edition")]
    public Level level;
    [Header("Note selection")]
    public int selectedNoteIndex;
    public NoteInstance selectedNoteInstance;

    [HideInInspector] public string notePlacementResourceFolder;
    [HideInInspector] public string musicSheetResourceFolder;

    private LevelEditorGUI GUI;
    private MusicPlayer musicPlayer;
    private NoteGrid grid;
    private NoteSpawner noteSpawner;
    private Trombone trombone;
    private HandCursor cursor;
    private float scrollSpeed;
    private float timeScrollValue;

    SheetMusic LoadedMusic => level != null ? level.music : null;

    private void Awake()
    {
        GUI = FindObjectOfType<LevelEditorGUI>(true);
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        noteSpawner = FindObjectOfType<NoteSpawner>(true);
        grid = FindObjectOfType<NoteGrid>(true);
        trombone = FindObjectOfType<Trombone>(true);
        cursor = FindObjectOfType<HandCursor>(true);
    }

    private void OnEnable()
    {
        if (GUI != null)
        {
            GUI.GUIActive = true;
            GUI.onMoveTimeBar.AddListener(OnMoveTimeBar);
        }
    }

    private void OnDisable()
    {
        if (GUI != null)
        {
            GUI.GUIActive = false;
            GUI.onMoveTimeBar.RemoveListener(OnMoveTimeBar);
        }
    }

    private void Start()
    {
        //if (trombone != null)
        //    trombone.autoSettings.mode = TromboneAutoSettings.Mode.AutoBlow | TromboneAutoSettings.Mode.AutoTone;
    }

    private void Update()
    {
        if (noteSpawner != null)
            noteSpawner.enabled = true;
        if (musicPlayer != null)
        {
            if (musicPlayer.music != LoadedMusic)
            {
                noteSpawner.ClearNotes();
                musicPlayer.LoadMusic(LoadedMusic, null, trombone.Sampler);
                musicPlayer.playTime = 0f;
                if (LoadedMusic != null) GUI.SetTimeBar(0f, LoadedMusic.DurationSeconds);
            }
            if (KeyControls() == false)
            {
                CursorControls();
                ScrollControls();
            }
        }
    }

    private void OnMoveTimeBar(float value)
    {
        if (musicPlayer != null)
        {
            timeScrollValue = value * musicPlayer.MusicDuration;
            scrollSpeed = scrollDuration > 0f ? (timeScrollValue - musicPlayer.playTime) / scrollDuration : fastPlaySpeed;
        }
    }

    private void SetPlayTime(float time)
    {
        musicPlayer.playTime = time;
        if (GUI != null && LoadedMusic != null) GUI.SetTimeBar(musicPlayer.playTime, LoadedMusic.DurationSeconds);
    }

    private void CursorControls()
    {
        if (cursor != null)
        {
            if (cursor.MainClick)
            {
                Vector2 clickPosition = cursor.HandPosition;
                NoteInstance[] spawnedNotes = noteSpawner.GetComponentsInChildren<NoteInstance>();
                if (spawnedNotes.Length > 0)
                {
                    NoteInstance closestNote = spawnedNotes[0];
                    float distance = Vector2.Distance(closestNote.transform.position, clickPosition);
                    foreach (NoteInstance n in spawnedNotes)
                    {
                        if (Vector2.Distance(n.transform.position, clickPosition) < distance)
                        {
                            closestNote = n;
                            distance = Vector2.Distance(n.transform.position, clickPosition);
                        }
                    }
                    selectedNoteInstance = closestNote;
                    //selectedNoteIndex = Array.FindIndex(musicPlayer.music.notes, noteInfo => noteInfo.startTime == selectedNoteInstance.info.startTime);
                }
            }
        }
    }

    private bool ScrollControls()
    {
        if (scrollSpeed == 0f)
        {
            musicPlayer.Pause();
            SetPlayTime(musicPlayer.playTime);
        }
        else
        {
            if ((scrollSpeed > 0 && musicPlayer.playTime < timeScrollValue) || (scrollSpeed < 0 && musicPlayer.playTime > timeScrollValue))
            {
                musicPlayer.playingSpeed = scrollSpeed;
                musicPlayer.Play();
                return true;
            }
            else
                scrollSpeed = 0f;
        }
        return false;
    }

    public void SetNoteInfo(NoteInfo note)
    {
        Debug.LogError("not implemented!");
        //musicPlayer.music.notes[selectedNoteIndex] = note;
    }

    //public void RespawnNote()
    //{
    //    Color noteColor = selectedNoteInstance.display.baseColor;
    //    DestroyImmediate(selectedNoteInstance.gameObject);
    //    //selectedNoteInstance = noteSpawner.SpawnNote(musicPlayer.music.notes[selectedNoteIndex]);
    //    selectedNoteInstance.display.baseColor = noteColor;
    //}

    public void AlternateNoteCoordinates()
    {
        if (selectedNoteInstance == null) return;
        NoteInfo note = NoteInfo.GetInfo(selectedNoteInstance);
        Vector2[] possibleCoordinates = grid.dimensions.ToneToCoordinates(note.tone);
        Vector2 currentCoordinates = grid.WorldPositionToCoordinates(selectedNoteInstance.transform.position);
        int coordinatesIndex = Array.FindIndex(possibleCoordinates, c => c == currentCoordinates);
        coordinatesIndex = (coordinatesIndex + 1) % possibleCoordinates.Length;
        //note.gridCoordinates = possibleCoordinates[coordinatesIndex];
        SetNoteInfo(note);
    }

    public void ChangeNoteLength(float delta)
    {
        NoteInfo note = NoteInfo.GetInfo(selectedNoteInstance);
        note.duration += delta;
        SetNoteInfo(note);
    }

    private bool KeyControls()
    {
        // Zoom controls
        //if (Input.GetKey(KeyCode.UpArrow))
        //    cameraPlacer.Zoom(zoomSpeed * Time.deltaTime);
        //else if (Input.GetKey(KeyCode.DownArrow))
        //    cameraPlacer.Zoom(-zoomSpeed * Time.deltaTime);
        // Play controls
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            musicPlayer.playingSpeed = Input.GetKey(KeyCode.LeftControl) ? -fastPlaySpeed : - 1f;
            musicPlayer.Play();
            return true;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            musicPlayer.playingSpeed = Input.GetKey(KeyCode.LeftControl) ? fastPlaySpeed : 1f;
            musicPlayer.Play();
            return true;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            musicPlayer.Pause();
            return true;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            AlternateNoteCoordinates();
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ChangeNoteLength(lengthChangeSpeed);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ChangeNoteLength(-lengthChangeSpeed);
        }
        return false;
    }
}
