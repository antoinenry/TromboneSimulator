using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct MetronomeTrack
{
    public float[] beatTimes;
    public float[] barTimes;
    [Header("Limit Values")]
    public float startBeatDuration;
    public int startBeatsPerBar;
    public float endBeatDuration;
    public int endBeatsPerBar;

    public float StartBarDuration => startBeatDuration * startBeatsPerBar;
    public float EndBarDuration => endBeatsPerBar * endBeatDuration;
    public float TrackDuration
    {
        get
        {
            int beatTrackLength = beatTimes != null ? beatTimes.Length : 0;
            return beatTrackLength > 1 ? beatTimes[beatTrackLength - 1] - beatTimes[0] + endBeatDuration : 0f;
        }

    }

    #region Track setup
    public void SetRythm(TempoInfo[] tempoChanges, MeasureInfo[] measureChanges)
    {
        // Generate beat and bar times from tempo and measure changes
        List<float> getBeatTimes = new List<float>();
        List<float> getBarTimes = new List<float>();
        int tempoChangesCount = tempoChanges != null ? tempoChanges.Length : 0;
        int measureChangesCount = measureChanges != null ? measureChanges.Length : 0;
        // Initialize tempo and measure
        TempoInfo tempoInfo = tempoChangesCount > 0 ? tempoChanges[0] : new TempoInfo(0f);
        MeasureInfo measureInfo = measureChangesCount > 0 ? measureChanges[0] : new MeasureInfo(0);
        // Get limit values
        startBeatDuration = tempoInfo.secondsPerQuarterNote * measureInfo.quarterNotesPerBeat;
        startBeatsPerBar = measureInfo.BeatsPerBar;
        endBeatDuration =
            (tempoChangesCount > 1 ? tempoChanges[tempoChangesCount - 1].secondsPerQuarterNote : tempoInfo.secondsPerQuarterNote)
            * (measureChangesCount > 1 ? measureChanges[measureChangesCount - 1].quarterNotesPerBeat : measureInfo.quarterNotesPerBeat);
        endBeatsPerBar = measureChangesCount > 1 ? measureChanges[measureChangesCount - 1].BeatsPerBar : startBeatsPerBar;
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
                beatTimes = null;
                barTimes = null;
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
                    getBeatTimes.Add(timeInSeconds);
                    // When a bar starts, add bar time
                    if (timeInBars % 1f == 0f) getBarTimes.Add(timeInSeconds);
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
        beatTimes = getBeatTimes.ToArray();
        barTimes = getBarTimes.ToArray();
    }
    #endregion

    #region Beat getters
    public int GetBeatIndex(float time)
    {
        int beatTrackLength = beatTimes != null ? beatTimes.Length : 0;
        if (beatTrackLength > 0)
        {
            // Find closest beat before time
            int getBeatIndex = Array.FindLastIndex(beatTimes, t => t <= time);
            if (getBeatIndex != -1)
            {
                // Beat is withing track range
                if (getBeatIndex < beatTrackLength - 1) return getBeatIndex;
                // Found beat is after the end of track: we need to extrapolate time and index
                else return beatTrackLength - 1 + Mathf.FloorToInt((time - beatTimes[beatTrackLength - 1]) / endBeatDuration);
            }
            // Time is before first beat: extrapolate
            else
            {
                float floatingBeatIndex = (time - beatTimes[0]) / startBeatDuration;
                return Mathf.FloorToInt(floatingBeatIndex);
            }
        }
        // Beat track is empty, return 0 as default index
        return 0;
    }

    public float GetBeatStartTime(int beatIndex)
    {
        int beatTrackLength = beatTimes != null ? beatTimes.Length : 0;
        if (beatIndex <= 0) return startBeatDuration * beatIndex;
        else if (beatIndex < beatTrackLength - 1) return beatTimes[beatIndex];
        else return endBeatDuration * (beatIndex - beatTrackLength + 1);
    }

    public float GetBeatDuration(int beatIndex)
    {
        int beatTrackLength = beatTimes != null ? beatTimes.Length : 0;
        if (beatIndex <= 0) return startBeatDuration;
        else if (beatIndex < beatTrackLength - 1) return beatTimes[beatIndex + 1] - beatTimes[beatIndex];
        else return endBeatDuration;
    }

    public BeatInfo GetBeat(int beatIndex) => new BeatInfo()
    {
        index = beatIndex,
        startTime = GetBeatStartTime(beatIndex),
        duration = GetBeatDuration(beatIndex)
    };

    public BeatInfo GetBeat(float time) => GetBeat(GetBeatIndex(time));
    #endregion

    #region Bar getters
    public int GetBarIndex(float time)
    {
        int barTrackLength = barTimes != null ? barTimes.Length : 0;
        if (barTrackLength > 0)
        {
            // Find closest bar before time
            int getBarIndex = Array.FindLastIndex(barTimes, t => t <= time);
            if (getBarIndex != -1)
            {
                // Bar is withing track range
                if (getBarIndex < barTrackLength - 1) return getBarIndex;
                // Found bar is after the end of track: we need to extrapolate time and index
                else return barTrackLength - 1 + Mathf.FloorToInt((time - barTimes[barTrackLength - 1]) / EndBarDuration);
            }
            // Time is before first bar: extrapolate
            else
            {
                float floatingBarIndex = (time - barTimes[0]) / StartBarDuration;
                return Mathf.FloorToInt(floatingBarIndex);
            }
        }
        // Bar track is empty, return 0 as default index
        return 0;
    }

    public float GetBarStartTime(int barIndex)
    {
        int barTrackLength = barTimes != null ? barTimes.Length : 0;
        if (barIndex <= 0) return StartBarDuration * barIndex;
        else if (barIndex < barTrackLength - 1) return barTimes[barIndex];
        else return EndBarDuration * (barIndex - barTrackLength + 1);
    }

    public float GetBarDuration(int barIndex)
    {
        int barTrackLength = barTimes != null ? barTimes.Length : 0;
        if (barIndex <= 0) return StartBarDuration;
        else if (barIndex < barTrackLength - 1) return barTimes[barIndex + 1] - barTimes[barIndex];
        else return EndBarDuration;
    }

    public BarInfo GetBar(int barIndex)
    {
        BarInfo bar = new BarInfo()
        {
            index = barIndex,
            startTime = GetBarStartTime(barIndex),
            durationInSeconds = GetBarDuration(barIndex)
        };
        bar.startBeatIndex = GetBeatIndex(bar.startTime);
        bar.durationInBeats = GetBeatIndex(bar.startTime + bar.durationInSeconds) - bar.startBeatIndex;
        return bar;
    }

    public BarInfo GetBar(float time) => GetBar(GetBarIndex(time));
    #endregion
}