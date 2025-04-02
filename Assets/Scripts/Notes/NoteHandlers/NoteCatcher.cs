using UnityEngine;
using UnityEngine.Events;

public class NoteCatcher : MonoBehaviour
{
    public bool showDebug;
    [Header("Components")]
    public NoteSpawner spawner;
    public TromboneCore trombone;
    public Playhead<INoteInfo> playhead;
    [Header("Activation")]
    public bool catchNotes;
    public float catchTone;
    public float startDelayTolerance;
    public float endAdvanceTolerance;
    public float toneTolerance;
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
                pos + scale * endAdvanceTolerance * Vector3.right,
                pos + scale * startDelayTolerance * Vector3.left);
        }
    }

    private void OnEnable()
    {
        if (spawner != null)
            //spawner.OnMoveNote.AddListener(OnNoteMoved);
            spawner.onMoveNotes.AddListener(OnNotesMoved);
        if (playhead != null)
        {
            playhead.onRead.AddListener(OnNoteIsInPlayheadRange);
            playhead.onExitRead.AddListener(OnNoteExitsCatchRange);
            playhead.onEndExitRead.AddListener(OnNoteEndExitsCatchRange);
        }
    }

    private void OnDisable()
    {
        if (spawner != null)
            //spawner.OnMoveNote.RemoveListener(OnNoteMoved);
            spawner.onMoveNotes.RemoveListener(OnNotesMoved);
        if (playhead != null)
        {
            playhead.onRead.RemoveListener(OnNoteIsInPlayheadRange);
            playhead.onExitRead.RemoveListener(OnNoteExitsCatchRange);
            playhead.onEndExitRead.RemoveListener(OnNoteEndExitsCatchRange);
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
        CatchStartTime = -Mathf.Max(startDelayTolerance, frameDeltaTime);
        CatchEndTime = Mathf.Max(endAdvanceTolerance, frameDeltaTime);
        // Set playhead width according to catch range
        playhead.MinimumTime = CatchStartTime;
        playhead.MaximumTime = CatchEndTime;
        // Read notes
        playhead.Move(notes, fromTime, toTime);
    }

    private void OnNoteIsInPlayheadRange(int noteIndex, INoteInfo note)
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
                float toneError = Mathf.Abs(catchTone - info.tone), accuracy = Mathf.Max(0f, 1f - toneError);
                // Correct tone
                if (toneError <= toneTolerance)
                {
                    if (showDebug) Debug.Log("-> Catching");
                    instance.PlayCorrectly(playhead.PreviousTime + CatchStartTime, playhead.CurrentTime + CatchEndTime, accuracy);
                    if (playhead.PreviousTime > info.startTime) onCorrectNote.Invoke(instance);
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

    private void OnNoteExitsCatchRange(int noteIndex, INoteInfo note)
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

    private void OnNoteEndExitsCatchRange(int noteIndex, INoteInfo note)
    {
        if (note != null && note is NoteSpawn)
        {
            onNoteCatchEnd.Invoke(note as NoteSpawn);
        }
    }
}
