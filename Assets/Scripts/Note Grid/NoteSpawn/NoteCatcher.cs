using UnityEngine;
using UnityEngine.Events;

public class NoteCatcher : MonoBehaviour
{
    public bool showDebug;
    [Header("Components")]
    public NoteSpawner spawner;
    public TromboneCore trombone;
    public Playhead playhead;
    [Header("Activation")]
    public bool catchNotes;
    public float catchTone;
    public float advanceTolerance;
    public float delayTolerance;
    public float toneTolerance;
    [Header("Look")]
    public Color catchColor = Color.yellow;
    public Color missColor = Color.red;
    [Header("Events")]
    public UnityEvent<NoteSpawn> onCorrectNote;
    public UnityEvent<NoteSpawn> onFullCorrectNote;
    public UnityEvent<NoteSpawn> onWrongNote;
    public UnityEvent<NoteSpawn> onMissNote;
    public UnityEvent<NoteSpawn> onNoteCatchEnd;

    public float CatchStartTime { get; private set; }
    public float CatchEndTime { get; private set; }

    private void OnDrawGizmosSelected()
    {
        if (spawner != null)
        {
            Vector3 pos = spawner.transform.position;
            float scale = spawner.TimeScale;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                pos + scale * advanceTolerance * Vector3.right,
                pos + scale * delayTolerance * Vector3.left);
        }
    }

    private void OnEnable()
    {
        if (spawner != null)
            //spawner.OnMoveNote.AddListener(OnNoteMoved);
            spawner.onMoveNotes.AddListener(OnNotesMoved);
        if (playhead != null)
        {
            playhead.onNote.AddListener(OnNoteIsInCatchTimeRange);
            playhead.onExitNote.AddListener(OnNoteExitsCatchRange);
            playhead.onEndExitNote.AddListener(OnNoteEndExitsCatchRange);
        }
    }

    private void OnDisable()
    {
        if (spawner != null)
            //spawner.OnMoveNote.RemoveListener(OnNoteMoved);
            spawner.onMoveNotes.RemoveListener(OnNotesMoved);
        if (playhead != null)
        {
            playhead.onNote.RemoveListener(OnNoteIsInCatchTimeRange);
            playhead.onExitNote.RemoveListener(OnNoteExitsCatchRange);
            playhead.onEndExitNote.RemoveListener(OnNoteEndExitsCatchRange);
        }
    }

    private void OnNotesMoved(NoteSpawn[] notes, float fromTime, float toTime)
    {
        if (notes == null || playhead == null) return;
        // Trombone controls catcher
        if (trombone != null)
        {
            catchNotes = trombone.blow;
            catchTone = trombone.Tone;
        }
        // Set catch range : time tolerance should be at least greater than frame time
        float frameDeltaTime = Time.deltaTime;
        CatchStartTime = -Mathf.Max(delayTolerance, frameDeltaTime);
        CatchEndTime = Mathf.Max(advanceTolerance, frameDeltaTime);
        // Set playhead width according to catch range
        playhead.MinimumTime = CatchStartTime;
        playhead.MaximumTime = CatchEndTime;
        // Read notes
        playhead.Move(notes, fromTime, toTime);
    }

    private void OnNoteIsInCatchTimeRange(int noteIndex, INote note)
    {
        if (note != null && note is NoteSpawn)
        {
            NoteSpawn instance = note as NoteSpawn;
            NoteInfo info = NoteInfo.GetInfo(note);
            if (showDebug) Debug.Log(instance + " is in catch range");
            // Full catch
            if (instance.catchState != NoteSpawn.CatchState.All && info.EndTime <= playhead.PreviousTime)
            {
                if (instance.performance.CorrectTime == info.duration)
                {
                    if (showDebug) Debug.Log("-> Full catch");
                    instance.FullCatch();
                    onFullCorrectNote.Invoke(instance);
                }
            }
            // Catch note
            else if (catchNotes)
            {
                float accuracy = toneTolerance > 0f ? 1f - Mathf.Abs(catchTone - info.tone) / toneTolerance : 0f;
                // Correct tone
                if (accuracy > 0f)
                {
                    if (showDebug) Debug.Log("-> Catching");
                    instance.PlayCorrectly(playhead.PreviousTime + CatchStartTime, playhead.CurrentTime + CatchEndTime, accuracy);
                    onCorrectNote.Invoke(instance);
                }
                // Wrong tone
                else
                {
                    // Detect wrong note only at current time (ignore playhead width / catch tolerance)
                    if (playhead.PreviousTime > info.startTime)
                    {
                        if (showDebug) Debug.Log("-> Wrong tone: " + catchTone + " / " + info.tone);
                        instance.PlayWrong(playhead.PreviousTime + CatchStartTime, playhead.CurrentTime, accuracy);
                        onWrongNote.Invoke(instance);
                    }
                }
            }
        }        
    }

    private void OnNoteExitsCatchRange(int noteIndex, INote note)
    {
        if (note != null && note is NoteSpawn)
        {
            //// Miss parts of note that haven't been caught
            //note.Miss(playhead.PreviousTime, playhead.CurrentTime + CatchEndTime, out FloatSegment missedSegment);
            //// Parts are being missed
            //if (missedSegment.Length > 0f)
            //{
            //    if (showDebug) Debug.Log("-> Missed");
            //    onMissNote.Invoke(note);
            //}
            //else
            //{
            //    if (showDebug) Debug.Log("-> Caught");
            //}
        }
    }

    private void OnNoteEndExitsCatchRange(int noteIndex, INote note)
    {
        if (note != null && note is NoteSpawn)
        {
            onNoteCatchEnd.Invoke(note as NoteSpawn);
        }
    }
}
