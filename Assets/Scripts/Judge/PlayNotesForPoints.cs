using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static NoteSpawn;
using static PlayNotesForPointsGUI;

public class PlayNotesForPoints : MonoBehaviour
{
    [Header("Components")]
    public PlayNotesForPointsGUI GUI;
    [Header("Look")]
    public Color fullPlayGaugeColor = Color.yellow;
    public float fullPlayAnimationDuration = 1f;
    [Header("Configuration")]
    public int noteCount = 10;
    public bool fullNotesOnly = true;
    public int basePointsPerNote = 10;
    public float multiplierPerNote = 1f;

    public UnityEvent onAllNotesPlayed;

    private PerformanceJudge judge;
    private int playedNoteCounter;
    private int missedNoteCounter;


    private void Awake()
    {
        judge = FindObjectOfType<PerformanceJudge>(true);
    }

    private void OnEnable()
    {
        ResetCounter();
        GUI?.SetAllNoteStates(NoteState.Default);
        judge?.onNotePerformanceEnd?.AddListener(OnNoteEnd);
    }

    private void OnDisable()
    {
        judge?.onNotePerformanceEnd?.RemoveListener(OnNoteEnd);
    }

    private void Update()
    {
        GUI?.SetGaugeLength(noteCount);
    }

    public int CurrentNoteIndex => playedNoteCounter + missedNoteCounter;

    private void OnNoteEnd(NoteSpawn note, float noteScore)
    {
        if (note == null) return;
        if (note.catchState == CatchState.All || (!fullNotesOnly && note.catchState == CatchState.Mixed))
            AddPlayedNote();
        else
            AddMissedNote();
    }

    private void AddPlayedNote()
    {
        GUI?.SetNoteStateAt(CurrentNoteIndex, NoteState.Played);
        playedNoteCounter++;
        int points = (int)(playedNoteCounter * multiplierPerNote * basePointsPerNote);
        judge?.AddScore(points);
        GUI?.SetPoints(points);
        if (playedNoteCounter >= noteCount) OnFullPlay();
    }

    private void AddMissedNote()
    {
        GUI?.SetNoteStateAt(CurrentNoteIndex, NoteState.Missed);
        missedNoteCounter++;
    }

    private void OnFullPlay()
    {
        onAllNotesPlayed.Invoke();
        StartCoroutine(FullPlayAnimationCoroutine());
    }

    private IEnumerator FullPlayAnimationCoroutine()
    {
        if (GUI == null) yield break;
        if (fullPlayAnimationDuration <= 0f || noteCount == 0)
        {
            GUI.SetAllNoteStates(NoteState.AllPlayed);
            yield break;
        }
        float delayBetweenNodes = fullPlayAnimationDuration / noteCount;
        for (int i = 0; i < noteCount; i++)
        {
            GUI.SetNoteStateAt(i, NoteState.AllPlayed);
            yield return new WaitForSeconds(delayBetweenNodes);
        }
    }

    public void ResetCounter()
    {
        playedNoteCounter = 0;
        missedNoteCounter = 0;
    }
}
