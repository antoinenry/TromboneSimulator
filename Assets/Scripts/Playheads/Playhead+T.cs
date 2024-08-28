using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public abstract class Playhead<T> : Playhead where T : ITimingInfo
{
    public UnityEvent<int, T> onStartEnterRead;
    public UnityEvent<int, T> onEnterRead;
    public UnityEvent<int, T> onStartExitRead;
    public UnityEvent<int, T> onRead;
    public UnityEvent<int, T> onEndEnterRead;
    public UnityEvent<int, T> onExitRead;
    public UnityEvent<int, T> onEndExitRead;

    protected List<T> currentReads;

    public T CurrentRead => currentReads.Count > 0 ? currentReads[0] : (T)ITimingInfo.None;

    protected void Awake()
    {
        currentReads = new List<T>();
    }

    public override void Stop()
    {
        base.Stop();
        Clear();
    }

    public override void Clear()
    {
        currentReads?.Clear();
    }

    public ReadProgress[] Move(T[] readableInfos, float fromTime, float toTime, bool offsetFromTime = true, bool offsetToTime = true, bool events = true)
    {
        // Progress array
        int readableCount = readableInfos != null ? readableInfos.Length : 0;
        ReadProgress[] progress = new ReadProgress[readableCount];
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
        // Read events
        if (readableCount > 0)
        {
            // Times with offset
            float fromTime_offset = offsetFromTime ? fromTime + timeOffset : fromTime;
            float toTime_offset = offsetToTime ? toTime + timeOffset : toTime;
            // Read straight
            if (!loop)
            {
                playedLoopCount = 0;
                Read(readableInfos, fromTime_offset, toTime_offset, ref progress, events);
            }
            // Read on loop
            else
            {
                if (LoopWidth != 0f) playedLoopCount = Mathf.FloorToInt((toTime_offset - loopStart) / LoopWidth);
                else playedLoopCount = 0;
                bool reverse = fromTime_offset > toTime_offset;
                // Wrap time values inside the loop
                float fromTime_looped = LoopTime(fromTime_offset);
                float toTime_looped = LoopTime(toTime_offset);
                // If time order is the same, it means no looping has occured
                if (reverse == fromTime_looped > toTime_looped)
                {
                    Read(readableInfos, fromTime_looped, toTime_looped, ref progress, events);
                }
                // If time has looped during deltatime, we read in two steps (before and after looping)
                else
                {
                    // Regular time
                    if (!reverse)
                    {
                        Read(readableInfos, fromTime_looped, loopEnd, ref progress, events);
                        Read(readableInfos, loopStart, toTime_looped, ref progress, events);
                    }
                    // Reversed time
                    else
                    {
                        Read(readableInfos, fromTime_looped, loopStart, ref progress, events);
                        Read(readableInfos, loopEnd, toTime_looped, ref progress, events);
                    }
                }
            }
        }
        // Done
        return progress;
    }

    protected void Read(T[] readableInfos, float fromTime, float toTime, ref ReadProgress[] progress, bool events, bool addLoopedTimeToReadableInfo = false)
    {
        if (readableInfos != null)
        {
            for (int n = 0, nCount = readableInfos.Length; n < nCount; n++)
                progress[n] = Read(n, readableInfos[n], fromTime, toTime, events, addLoopedTimeToReadableInfo);
        }
    }

    protected ReadProgress Read(int noteIndex, T readableInfo, float fromTime, float toTime, bool events, bool addLoopedTimeToReadableInfo = false)
    {
        if (readableInfo == null) return ReadProgress.None;
        // Get info time segment
        //ITimingInfo readableInfo = ITimingInfo.GetInfo(readable);
        float startTime = readableInfo.StartTime;
        if (addLoopedTimeToReadableInfo) startTime += playedLoopCount * LoopWidth;
        FloatSegment noteSegment = new FloatSegment(startTime, readableInfo.EndTime);
        // Playhead range at t = fromTime and at t = toTime
        float halfWidth = timeWidth / 2f;
        FloatSegment fromPlayheadRange = new FloatSegment(fromTime - halfWidth, fromTime + halfWidth);
        FloatSegment toPlayheadRange = new FloatSegment(toTime - halfWidth, toTime + halfWidth);
        FloatSegment totalPlayheadRange = new FloatSegment(fromPlayheadRange.start, toPlayheadRange.end);
        // Get playhead range movement relative to note
        ReadProgress progress;
        // Note is whithin playhead range
        if (totalPlayheadRange.Crosses(noteSegment))
        {
            progress = ReadProgress.IsOnTime;
            // Check start of note
            if (totalPlayheadRange.Contains(noteSegment.start))
            {
                progress |= ReadProgress.IsEnteringRead;
                if (fromPlayheadRange.Contains(noteSegment.start) == false) progress |= ReadProgress.StartsEnteringRead;
                if (toPlayheadRange.Contains(noteSegment.start) == false) progress |= ReadProgress.EndsEnteringRead;
            }
            // Check end of note
            if (totalPlayheadRange.Contains(noteSegment.end))
            {
                progress |= ReadProgress.IsExitingRead;
                if (fromPlayheadRange.Contains(noteSegment.end) == false) progress |= ReadProgress.StartsExitingRead;
                if (toPlayheadRange.Contains(noteSegment.end) == false) progress |= ReadProgress.EndsExitingRead;
            }
        }
        // Note is completly out of range
        else
        {
            if (totalPlayheadRange.start > noteSegment.end) progress = ReadProgress.IsAfterTime;
            else progress = ReadProgress.IsBeforeTime;
        }
        // On note
        if (progress.HasFlag(ReadProgress.IsOnTime))
        {
            if (showDebug)
            {
                Debug.Log(name + " (" + fromTime + "s - " + toTime + "s) is on note " + readableInfo);
                Debug.Log("-> progress: " + progress);
            }
            // Add to current notes (avoid doublons)
            if (currentReads == null) currentReads = new List<T>();
            if (currentReads.Contains(readableInfo) == false) currentReads.Add(readableInfo);
            // Events (in time order)
            if (events)
            {
                if (progress.HasFlag(ReadProgress.StartsEnteringRead)) onStartEnterRead.Invoke(noteIndex, readableInfo);
                if (progress.HasFlag(ReadProgress.IsEnteringRead)) onEnterRead.Invoke(noteIndex, readableInfo);
                if (progress.HasFlag(ReadProgress.EndsEnteringRead)) onEndEnterRead.Invoke(noteIndex, readableInfo);
                onRead.Invoke(noteIndex, readableInfo);
                if (progress.HasFlag(ReadProgress.StartsExitingRead)) onStartExitRead.Invoke(noteIndex, readableInfo);
                if (progress.HasFlag(ReadProgress.IsExitingRead)) onExitRead.Invoke(noteIndex, readableInfo);
                if (progress.HasFlag(ReadProgress.EndsExitingRead)) onEndExitRead.Invoke(noteIndex, readableInfo);
            }
        }
        // Note on note
        else
        {
            // Remove from current notes
            currentReads?.Remove(readableInfo);
        }
        // Done reading
        return progress;
    }
}