using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewPlayhead", menuName = "Trombone Hero/Music/Playhead")]
public class Playhead : ScriptableObject
{
    [Flags]
    public enum ProgressOnNote
    { 
        None = 0,
        IsBeforeNote = 1,
        StartsEnteringNote = 2,
        IsEnteringNote = 4,
        EndsEnteringNote = 8,
        IsOnNote = 16,
        StartsExitingNote = 32,
        IsExitingNote = 64,
        EndsExitingNote = 128,
        IsAfterNote = 256
    }

    public bool showDebug = false;
    [Header("Timing")]
    public float timeOffset = 0f;
    public float timeWidth = 0f;
    [Header("Loop")]
    public bool loop = false;
    public float loopStart = 0f;
    public float loopEnd = 0f;
    public int playedLoopCount = 0;
    [Header("Events")]
    public UnityEvent<float,float> onMove;
    public UnityEvent<float> onPause;
    public UnityEvent onStop;
    public UnityEvent<int, INote> onStartEnterNote;
    public UnityEvent<int, INote> onEnterNote;
    public UnityEvent<int, INote> onStartExitNote;
    public UnityEvent<int, INote> onNote;
    public UnityEvent<int, INote> onEndEnterNote;
    public UnityEvent<int, INote> onExitNote;
    public UnityEvent<int, INote> onEndExitNote;

    private List<NoteInfo> currentNoteInfos;

    public NoteInfo CurrentNoteInfo => currentNoteInfos.Count > 0? currentNoteInfos[0] : NoteInfo.None;    
    public float PreviousTime { get; private set; }
    public float CurrentTime { get; private set; }
    public float DeltaTime { get; private set; }
    public float PlayingSpeed { get; private set; }
    public float LoopWidth => loopEnd - loopStart;

    public float MinimumTime
    {
        get
        {
            return timeOffset - timeWidth / 2f;
        }
        set
        {
            timeWidth = MaximumTime - value;
            timeOffset = value + timeWidth / 2f;
        }
    }

    public float MaximumTime
    {
        get
        {
            return timeOffset + timeWidth / 2f;
        }
        set
        {
            timeWidth = value - MinimumTime;
            timeOffset = value - timeWidth / 2f;
        }
    }

    private void Awake()
    {
        currentNoteInfos = new List<NoteInfo>();
    }

    public void Clear()
    {
        currentNoteInfos.Clear();
    }

    public ProgressOnNote[] Move(INote[] noteHandles, float fromTime, float toTime, bool offsetFromTime = true, bool offsetToTime = true, bool events = true)
    {
        // Progress array
        int noteCount = noteHandles != null ? noteHandles.Length : 0;
        ProgressOnNote[] progress = new ProgressOnNote[noteCount];
        // Time skip
        if (fromTime != CurrentTime)
        {
            // Do something here? Maybe just an event
            if (showDebug) Debug.Log(name + " time skip from " + CurrentTime + " to " + fromTime);
        }
        // Update playing time, deltatime and speed
        CurrentTime = toTime;
        PreviousTime = fromTime;
        DeltaTime = toTime - fromTime;
        float applicationDeltaTime = Time.deltaTime;
        if (applicationDeltaTime > 0f) PlayingSpeed = DeltaTime / applicationDeltaTime;
        else PlayingSpeed = 0f;
        // Trigger event
        if (DeltaTime != 0) onMove.Invoke(fromTime, toTime);
        else onPause.Invoke(toTime);
        // Read notes
        if (noteCount > 0)
        {
            // Times with offset
            float fromTime_offset = offsetFromTime ? fromTime + timeOffset : fromTime;
            float toTime_offset = offsetToTime ? toTime + timeOffset : toTime;
            // Read notes straight
            if (!loop)
            {
                playedLoopCount = 0;
                ReadNotes(noteHandles, fromTime_offset, toTime_offset, ref progress, events);
            }
            // Read note on loop
            else
            {
                if (LoopWidth != 0f) playedLoopCount = Mathf.FloorToInt((toTime_offset - loopStart) / LoopWidth);
                else playedLoopCount = 0;
                //ReadNotesLoop(notes, fromTime, toTime, speed, offsetFromTime, offsetToTime);
                bool reverse = fromTime_offset > toTime_offset;
                // Wrap time values inside the loop
                float fromTime_looped = LoopTime(fromTime_offset);
                float toTime_looped = LoopTime(toTime_offset);
                // If time order is the same, it means no looping has occured
                if (reverse == fromTime_looped > toTime_looped)
                {
                    ReadNotes(noteHandles, fromTime_offset, toTime_offset, ref progress, events);
                }
                // If time has looped during deltatime, we read in two steps (before and after looping)
                else
                {
                    // Regular time
                    if (!reverse)
                    {
                        ReadNotes(noteHandles, fromTime_offset, loopEnd, ref progress, events);
                        ReadNotes(noteHandles, loopStart, toTime_offset, ref progress, events);
                    }
                    // Reversed time
                    else
                    {
                        ReadNotes(noteHandles, fromTime_offset, loopStart, ref progress, events);
                        ReadNotes(noteHandles, loopEnd, toTime_offset, ref progress, events);
                    }
                }
            }
        }        
        // Done
        return progress;
    }

    private void ReadNotes(INote[] notes, float fromTime, float toTime, ref ProgressOnNote[] progress, bool events)
    {
        if (notes != null)
        {
            for (int n = 0, nCount = notes.Length; n < nCount; n++)
                progress[n] = ReadNote(n, notes[n], fromTime, toTime, events);
        }
    }

    private ProgressOnNote ReadNote(int noteIndex, INote noteHandle, float fromTime, float toTime, bool events)
    {
        if (noteHandle == null) return ProgressOnNote.None;
        // Get note time segment
        NoteInfo nInfo = NoteInfo.GetInfo(noteHandle);
        if (loop) nInfo.startTime += playedLoopCount * LoopWidth;
        FloatSegment noteSegment = new FloatSegment(nInfo.startTime, nInfo.EndTime);
        // Playhead range at t = fromTime and at t = toTime
        float halfWidth = timeWidth / 2f;
        FloatSegment fromPlayheadRange = new FloatSegment(fromTime - halfWidth, fromTime + halfWidth);
        FloatSegment toPlayheadRange = new FloatSegment(toTime - halfWidth, toTime + halfWidth);
        FloatSegment totalPlayheadRange = new FloatSegment(fromPlayheadRange.start, toPlayheadRange.end);
        // Get playhead range movement relative to note
        ProgressOnNote progress;
        // Note is whithin playhead range
        if (totalPlayheadRange.Crosses(noteSegment))
        {
            progress = ProgressOnNote.IsOnNote;
            // Check start of note
            if (totalPlayheadRange.Contains(noteSegment.start))
            {
                progress |= ProgressOnNote.IsEnteringNote;
                if (fromPlayheadRange.Contains(noteSegment.start) == false) progress |= ProgressOnNote.StartsEnteringNote;
                if (toPlayheadRange.Contains(noteSegment.start) == false) progress |= ProgressOnNote.EndsEnteringNote; 
            }
            // Check end of note
            if (totalPlayheadRange.Contains(noteSegment.end))
            {
                progress |= ProgressOnNote.IsExitingNote;
                if (fromPlayheadRange.Contains(noteSegment.end) == false) progress |= ProgressOnNote.StartsExitingNote;
                if (toPlayheadRange.Contains(noteSegment.end) == false) progress |= ProgressOnNote.EndsExitingNote;
            }
        }
        // Note is completly out of range
        else
        {
            if (totalPlayheadRange.start > noteSegment.end) progress = ProgressOnNote.IsAfterNote;
            else progress = ProgressOnNote.IsBeforeNote;
        }
        // On note
        if (progress.HasFlag(ProgressOnNote.IsOnNote))
        {
            if (showDebug)
            {
                Debug.Log(name + " (" + fromTime + "s - " + toTime + "s) is on note " + nInfo);
                Debug.Log("-> progress: " + progress);
            }
            // Add to current notes (avoid doublons)
            if (currentNoteInfos.Contains(nInfo) == false) currentNoteInfos.Add(nInfo);
            // Events (in time order)
            if (events)
            {
                if (progress.HasFlag(ProgressOnNote.StartsEnteringNote)) onStartEnterNote.Invoke(noteIndex, noteHandle);
                if (progress.HasFlag(ProgressOnNote.IsEnteringNote)) onEnterNote.Invoke(noteIndex, noteHandle);
                if (progress.HasFlag(ProgressOnNote.EndsEnteringNote)) onEndEnterNote.Invoke(noteIndex, noteHandle);
                onNote.Invoke(noteIndex, noteHandle);
                if (progress.HasFlag(ProgressOnNote.StartsExitingNote)) onStartExitNote.Invoke(noteIndex, noteHandle);
                if (progress.HasFlag(ProgressOnNote.IsExitingNote)) onExitNote.Invoke(noteIndex, noteHandle);
                if (progress.HasFlag(ProgressOnNote.EndsExitingNote)) onEndExitNote.Invoke(noteIndex, noteHandle);
            }
        }
        // Note on note
        else
        {
            // Remove from current notes
            currentNoteInfos.Remove(nInfo);
        }
        // Done reading
        return progress;
    }

    public float LoopTime(float t)
    {
        return Mathf.Repeat(t - loopStart, loopEnd - loopStart) + loopStart;
    }

    public void Stop()
    {
        //if (CurrentNoteInfo.silence == false) onEndEnterNote.Invoke(CurrentNoteInfo);
        onStop.Invoke();
        Clear();
    }
}