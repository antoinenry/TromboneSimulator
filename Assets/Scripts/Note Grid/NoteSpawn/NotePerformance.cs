using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NotePerformance
{
    public enum PlayState { None, PLAYED_CORRECTLY, PLAYED_WRONG, MISSED }

    [Serializable]
    public struct PerformanceSegment
    {
        public FloatSegment time;
        public PlayState playsState;
        public float toneError;

        public PerformanceSegment(float from, float to, PlayState state, float error = 0f)
        {
            time = new FloatSegment(from, to);
            playsState = state;
            toneError = error;
        }

        static public float TotalTime(IEnumerable<PerformanceSegment> segments)
        {
            float totalTime = 0f;
            if (segments != null)
                foreach (PerformanceSegment s in segments)
                    totalTime += s.time.Length;
            return totalTime;
        }

        static public float ToneErrorAverage(IEnumerable<PerformanceSegment> segments)
        {
            float timeSum = 0f;
            float errorSum = 0f;
            {
                foreach (PerformanceSegment s in segments)
                {
                    float timeLength = s.time.Length;
                    timeSum += timeLength;
                    errorSum += timeLength * s.toneError;
                }
            }
            return timeSum != 0f ? errorSum / timeSum : 0f;
        }
    }

    public List<PerformanceSegment> segments;

    public float TotalPerformanceTime => PerformanceSegment.TotalTime(segments);
    public List<PerformanceSegment> CorrectSegments => segments != null ? segments.FindAll(s => s.playsState == PlayState.PLAYED_CORRECTLY) : new List<PerformanceSegment>(0);
    public float CorrectTime => PerformanceSegment.TotalTime(CorrectSegments);
    public List<PerformanceSegment> WrongSegments => segments != null ? segments.FindAll(s => s.playsState == PlayState.PLAYED_WRONG) : new List<PerformanceSegment>(0);
    public float WrongTime => PerformanceSegment.TotalTime(WrongSegments);
    public List<PerformanceSegment> MissedSegments => segments != null ? segments.FindAll(s => s.playsState == PlayState.MISSED) : new List<PerformanceSegment>(0);
    public float MissedTime => PerformanceSegment.TotalTime(MissedSegments);

    public bool IsPlayStateAt(float t, params PlayState[] playStates)
    {
        if (segments != null)
            foreach (PerformanceSegment s in segments)
            {
                if (Array.IndexOf(playStates, s.playsState) != -1 && s.time.Contains(t))
                    return true;
            }
        return false;
    }

    public PerformanceSegment PlayCorrectly(float fromTime, float toTime, float toneError)
    {
        return TryAddSegment(new PerformanceSegment(fromTime, toTime, PlayState.PLAYED_CORRECTLY, toneError), true);
    }

    public PerformanceSegment PlayWrong(float fromTime, float toTime, float toneError)
    {
        return TryAddSegment(new PerformanceSegment(fromTime, toTime, PlayState.PLAYED_WRONG, toneError), false);
    }

    public PerformanceSegment Miss(float fromTime, float toTime)
    {
        return TryAddSegment(new PerformanceSegment(fromTime, toTime, PlayState.MISSED), false);
    }

    private PerformanceSegment TryAddSegment(PerformanceSegment segmentToAdd, bool replace)
    {
        if (segments == null) segments = new List<PerformanceSegment>();
        int segmentCount = segments.Count;
        int addSegmentAt = segmentCount;
        // Added segment can differ from the one passed in parameter, sometimes not added at all
        PerformanceSegment addedSegment = segmentToAdd;
        // Check if we can join with existing segments, beginning with last added
        if (segmentCount > 0)
        {
            for (int i = segmentCount - 1; i >= 0; i--)
            {
                PerformanceSegment lastSegment = segments[i];
                // New segment and last segment are connected
                if (lastSegment.time.Crosses(addedSegment.time))
                {
                    FloatSegment intersection = FloatSegment.Intersection(lastSegment.time, addedSegment.time);
                    // New segment is separate from last segment
                    if (intersection.IsNaN)
                    {
                        break;
                    }
                    // New segment is connected to last segment
                    else
                    {
                        // Segments are the same type
                        if (lastSegment.playsState == addedSegment.playsState)
                        {
                            // Join new and last segment with an average tone error
                            FloatSegment[] joinedTime = lastSegment.time.Join(addedSegment.time);
                            if (joinedTime != null && joinedTime.Length == 1)
                            {
                                PerformanceSegment joinedSegment = new PerformanceSegment();
                                joinedSegment.time = joinedTime[0];
                                joinedSegment.playsState = addedSegment.playsState;
                                joinedSegment.toneError = PerformanceSegment.ToneErrorAverage(new PerformanceSegment[2] { lastSegment, addedSegment });
                                addedSegment = joinedSegment;
                            }
                            else Debug.LogError("Add performance segment error.");
                        }
                        // Segments are different types
                        else
                        {
                            // Delete part of last segment
                            if (replace)
                            {
                                FloatSegment[] cutTime = lastSegment.time.Remove(intersection);
                                // Last segment is completly replaced by new segment
                                if (cutTime.Length == 0)
                                {

                                }
                                // Only part of last segment is replaced
                                else if (cutTime.Length == 1)
                                {
                                    // Cut replaced part
                                    lastSegment.time = cutTime[0];
                                    segments[i] = lastSegment;
                                    break;
                                    //segments.Add(newSegment);
                                }
                                // Last segment contains new segment (not implemented)
                                else if(cutTime.Length == 2)
                                {
                                    Debug.LogError("Add performance segment case not implemented");
                                    break;
                                    //segments.Add(newSegment);
                                }
                                // Problem
                                else Debug.LogError("Add performance segment error.");
                            }
                            // Delete part of new segment
                            else
                            {
                                FloatSegment[] cutTime = addedSegment.time.Remove(intersection);
                                // New segment is completly replaced by last segment
                                if (cutTime.Length == 0)
                                {
                                    // No segment is added
                                    addedSegment = new PerformanceSegment(0f, 0f, PlayState.None);
                                    break;
                                }
                                // Only part of new segment is replaced
                                if (cutTime.Length == 1)
                                {
                                    addedSegment.time = cutTime[0];
                                    //segments.Add(newSegment);
                                    break;
                                }
                                // New segment contains last segment (not implemented)
                                else if (cutTime.Length == 2)
                                {
                                    Debug.LogError("Add performance segment case not implemented");
                                    break;
                                    //segments.Add(newSegment);
                                }
                                // Problem
                                else Debug.LogError("Add performance segment error.");
                            }
                        }
                    }
                }
                // Replacing existing segments changes where we'll add the new segment
                addSegmentAt--;
            }
        }

        if (segmentToAdd.playsState == PlayState.PLAYED_CORRECTLY && segmentToAdd.toneError > .75f)
        {

        }

        if (addedSegment.playsState == PlayState.PLAYED_CORRECTLY && addedSegment.toneError > .75f)
        {

        }

        // Remove replaced segments
        if (addSegmentAt < segmentCount) segments.RemoveRange(addSegmentAt, segmentCount - addSegmentAt);
        // Add new segment if needed
        if (addedSegment.time.Length > 0f) segments.Add(addedSegment);
        // Return modified new segment
        return addedSegment;
    }
}