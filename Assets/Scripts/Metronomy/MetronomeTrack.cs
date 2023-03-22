using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct MetronomeTrack
{
    public float[] beatTimesSeconds;
    public float[] barTimesSeconds;

    public void SetRythm(TempoInfo[] tempoChanges, MeasureInfo[] measureChanges)
    {
        // Generate beat and bar times from tempo and measure changes
        List<float> beatTimes = new List<float>();
        List<float> barTimes = new List<float>();
        int tempoChangesCount = tempoChanges != null ? tempoChanges.Length : 0;
        int measureChangesCount = measureChanges != null ? measureChanges.Length : 0;
        // Initialize tempo and measure (default values at time and bar = 0)
        TempoInfo tempoInfo = new TempoInfo(0f);
        MeasureInfo measureInfo = new MeasureInfo(0);
        // Units to navigate through time, tempos and measures
        int tempoChangeIndex = 0;
        int measureChangeIndex = 0;
        float timeInSeconds = 0f;
        float timeInBeats = 0f;
        float timeInBars = 0f;
        // Unfold beats and bars
        bool noMoreTempoChanges = false;
        bool noMoreMeasureChanges = false;
        bool lastMeasure = false;
        while (noMoreTempoChanges == false || noMoreMeasureChanges == false || lastMeasure == false)
        {
            // Get tempo and measure
            if (tempoChangeIndex < tempoChangesCount) tempoInfo = tempoChanges[tempoChangeIndex];
            if (measureChangeIndex < measureChangesCount) measureInfo = measureChanges[measureChangeIndex];
            // Durations of beats and bars
            float beatDuration = tempoInfo.secondsPerQuarterNote * measureInfo.quarterNotesPerBeat;
            int beatsPerBar = measureInfo.BeatsPerBar;
            // Abort when those parameters are incorrect
            if (beatDuration <= 0f || beatsPerBar <= 0)
            {
                beatTimesSeconds = null;
                barTimesSeconds = null;
                return;
            }
            // Scope for next tempo change
            float nextTempoChangeSeconds;
            if (tempoChangeIndex < tempoChangesCount - 1)
                nextTempoChangeSeconds = tempoChanges[tempoChangeIndex + 1].time;
            else
            {
                // No more tempo changes ahead: keep current tempo until the end
                nextTempoChangeSeconds = float.PositiveInfinity;
                noMoreTempoChanges = true;
            }
            // Scope for next measure change
            int nextMeasureChangeBars;
            if (measureChangeIndex < measureChangesCount - 1)
                nextMeasureChangeBars = measureChanges[measureChangeIndex + 1].bar;
            else
            {
                // No more measure change: ensure we end with a complete measure at constant tempo
                if (noMoreTempoChanges)
                {
                    nextMeasureChangeBars = Mathf.CeilToInt(timeInBars) + 1;
                    lastMeasure = true;
                }
                else
                {
                    nextMeasureChangeBars = int.MaxValue;
                }
                noMoreMeasureChanges = true;
            }
            // Navigate through time to next change
            while (timeInSeconds < nextTempoChangeSeconds && timeInBars < nextMeasureChangeBars)
            {
                // When a beat starts, add beat time
                if (timeInBeats % 1f == 0f)
                {
                    beatTimes.Add(timeInSeconds);
                    // When a bar starts, add bar time
                    if (timeInBars % 1f == 0f) barTimes.Add(timeInSeconds);
                }
                // Time incrementation: next beat
                float timeStepBeats = 1f - (timeInBeats % 1f);
                float timeStepSeconds = timeStepBeats * beatDuration;
                // If next beat is at the same tempo, move to next beat
                if (timeInSeconds + timeStepSeconds < nextTempoChangeSeconds)
                {
                    timeInSeconds += timeStepSeconds;
                    timeInBeats = Mathf.Floor(timeInBeats + 1f);
                    timeInBars = (Mathf.Floor(beatsPerBar * timeInBars) + 1f) / beatsPerBar;
                }
                // If tempo changes between last and next beat, move to next tempo instead
                else
                {
                    float secondsLeftBeforeNewTempo = nextTempoChangeSeconds - timeInSeconds;
                    timeInSeconds = nextTempoChangeSeconds;
                    timeInBeats += secondsLeftBeforeNewTempo / beatDuration;
                    timeInBars += secondsLeftBeforeNewTempo / (beatDuration * (float)beatsPerBar);
                }
            }
            // Move change indices
            if (timeInSeconds >= nextTempoChangeSeconds) tempoChangeIndex++;
            if (timeInBars >= nextMeasureChangeBars) measureChangeIndex++;
        }
        // End
        beatTimesSeconds = beatTimes.ToArray();
        barTimesSeconds = barTimes.ToArray();
    }

    public void GetBeat(float atTime, out int beatIndex, out float beatTime, out float nextBeatTime)
    {
        // Default out values
        beatIndex = -1;
        beatTime = float.NaN;
        nextBeatTime = float.NaN;
        // Search for beat info in beat track
        int beatTrackLength = beatTimesSeconds != null ? beatTimesSeconds.Length : 0;
        // A beat track needs at least two beats
        if (beatTrackLength > 1)
        {
            // Find beat just before time
            beatIndex = Array.FindLastIndex(beatTimesSeconds, t => t <= atTime);
            if (beatIndex != -1)
            {
                // Beat was found
                beatTime = beatTimesSeconds[beatIndex];
                // Get next beat
                if (beatIndex + 1 < beatTrackLength)
                {
                    // Next beat is withing track range
                    nextBeatTime = beatTimesSeconds[beatIndex + 1];
                }
                else
                {
                    // Next beat is out of range: extrapolate
                    if (beatIndex > 0)
                    {
                        float beatDuration = beatTime - beatTimesSeconds[beatIndex - 1];
                        nextBeatTime = beatTime + beatDuration;
                    }
                    else Debug.LogWarning("Couldn't guess beat duration.");
                }
            }
            else
            {
                // No beat found (time is before first beat): extrapolate
                float beatDuration = beatTimesSeconds[1] - beatTimesSeconds[0];
                float floatingBeatIndex = (atTime - beatTimesSeconds[0]) / beatDuration;
                beatIndex = Mathf.FloorToInt(floatingBeatIndex);
                beatTime = beatTimesSeconds[0] + beatIndex * beatDuration;
                nextBeatTime = beatTime + beatDuration;
            }
        }
    }

    public void GetBeat(int beatIndex, out float beatTime, out float nextBeatTime)
    {
        // Default out values
        beatTime = float.NaN;
        nextBeatTime = float.NaN;
        // Search for beat info in beat track
        int beatTrackLength = beatTimesSeconds != null ? beatTimesSeconds.Length : 0;
        if (beatIndex >= 0 && beatIndex < beatTrackLength)
        {
            beatTime = beatTimesSeconds[beatIndex];
            if (beatIndex + 1 < beatTrackLength)
                nextBeatTime = beatTimesSeconds[beatIndex + 1];
            else if (beatIndex > 0)
            {
                // Extrapolate for next beat time
                float beatDuration = beatTime - beatTimesSeconds[beatIndex - 1];
                nextBeatTime = beatTime + beatDuration;
            }
            else Debug.LogWarning("Couldn't guess beat duration.");
        }
        else
        {
            // Extrapolate for beat time
            if (beatTrackLength > 1)
            {
                if (beatIndex < 0)
                {
                    float beatDuration = beatTimesSeconds[1] - beatTimesSeconds[0];
                    beatTime = beatTimesSeconds[0] + beatIndex * beatDuration;
                    nextBeatTime = beatTime + beatDuration;
                }
                else if (beatIndex >= beatTrackLength)
                {
                    float beatDuration = beatTimesSeconds[beatTrackLength - 1] - beatTimesSeconds[beatTrackLength - 2];
                    beatTime = beatTimesSeconds[beatTrackLength - 1] + (beatIndex - beatTrackLength) * beatDuration;
                    nextBeatTime = beatTime + beatDuration;
                }
            }
            else Debug.LogWarning("Couldn't guess beat duration.");
        }
    }

}