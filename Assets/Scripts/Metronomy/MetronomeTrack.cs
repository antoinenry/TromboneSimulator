using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct MetronomeTrack
{
    readonly public bool IsReady;
    readonly public int BeatCount;
    readonly public float StartBeatDuration;
    readonly public float EndBeatDuration;
    readonly public int BarCount;
    readonly public int StartBeatsPerBar;
    readonly public int EndBeatsPerBar;

    private TempoInfo[] tempoChanges;
    private MeasureInfo[] measureChanges;
    private float[] beatTimes;
    private float[] barTimes;

    public TempoInfo[] TempoChanges => tempoChanges != null ? Array.ConvertAll(tempoChanges, t => t) : null;
    public MeasureInfo[] MeasureChanges => measureChanges != null ? Array.ConvertAll(measureChanges, m => m) : null;
    public float FirstBeatTime => BeatCount > 0 ? beatTimes[0] : float.NaN;
    public float LastBeatTime => BeatCount > 0 ? beatTimes[BeatCount - 1] : float.NaN;
    public float StartBarDuration => StartBeatDuration * StartBeatsPerBar;
    public float EndBarDuration => EndBeatDuration * EndBeatsPerBar;
    public float TrackDuration
    {
        get
        {
            int beatTrackLength = beatTimes != null ? beatTimes.Length : 0;
            return beatTrackLength > 1 ? beatTimes[beatTrackLength - 1] - beatTimes[0] : 0f;
        }

    }

    public MetronomeTrack(TempoInfo[] tempos, MeasureInfo[] measures)
    {
        // Generate beat and bar times from tempo and measure changes
        List<float> getBeatTimes = new List<float>();
        List<float> getBarTimes = new List<float>();
        int tempoChangesCount = tempos != null ? tempos.Length : 0;
        int measureChangesCount = measures != null ? measures.Length : 0;
        // Initialize tempo and measure
        TempoInfo tempoInfo = tempoChangesCount > 0 ? tempos[0] : new TempoInfo(0f);
        MeasureInfo measureInfo = measureChangesCount > 0 ? measures[0] : new MeasureInfo(0);
        // Get limit values
        StartBeatDuration = tempoInfo.secondsPerQuarterNote * measureInfo.quarterNotesPerBeat;
        StartBeatsPerBar = measureInfo.BeatsPerBar;
        EndBeatDuration =
            (tempoChangesCount > 1 ? tempos[tempoChangesCount - 1].secondsPerQuarterNote : tempoInfo.secondsPerQuarterNote)
            * (measureChangesCount > 1 ? measures[measureChangesCount - 1].quarterNotesPerBeat : measureInfo.quarterNotesPerBeat);
        EndBeatsPerBar = measureChangesCount > 1 ? measures[measureChangesCount - 1].BeatsPerBar : StartBeatsPerBar;
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
            if (tempoChangeIndex < tempoChangesCount) tempoInfo = tempos[tempoChangeIndex];
            if (measureChangeIndex < measureChangesCount) measureInfo = measures[measureChangeIndex];
            // Durations of beats and bars
            float beatDuration = tempoInfo.secondsPerQuarterNote * measureInfo.quarterNotesPerBeat;
            int beatsPerBar = measureInfo.BeatsPerBar;
            // Abort when those parameters are incorrect
            if (beatDuration <= 0f || beatsPerBar <= 0)
            {
                tempoChanges = new TempoInfo[0];
                measureChanges = new MeasureInfo[0];
                IsReady = false;
                beatTimes = new float[0];
                BeatCount = 0;
                barTimes = new float[0];
                BarCount = 0;
                return;
            }
            // Scope for next tempo change
            float nextTempoChangeSeconds;
            if (tempoChangeIndex < tempoChangesCount - 1)
                nextTempoChangeSeconds = tempos[tempoChangeIndex + 1].time;
            else
            {
                // No more tempo changes ahead: keep current tempo until the end
                nextTempoChangeSeconds = float.PositiveInfinity;
                noMoreTempoChanges = true;
            }
            // Scope for next measure change
            int nextMeasureChangeBars;
            if (measureChangeIndex < measureChangesCount - 1)
                nextMeasureChangeBars = measures[measureChangeIndex + 1].bar;
            else
            {
                // No more measure change: ensure we end with a complete measure
                if (noMoreTempoChanges)
                {
                    nextMeasureChangeBars = Mathf.FloorToInt(timeInBars + 1f);
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
        // Finishing beat and bar
        getBeatTimes.Add(timeInSeconds);
        getBarTimes.Add(timeInSeconds);
        // End
        tempoChanges = Array.ConvertAll(tempos, t => t);
        measureChanges = Array.ConvertAll(measures, m => m);
        beatTimes = getBeatTimes.ToArray();
        BeatCount = getBeatTimes.Count;
        barTimes = getBarTimes.ToArray();
        BarCount = getBarTimes.Count;
        IsReady = true;
    }

    #region Beat getters
    public float[] GetBeatTimes(bool includeFinalBeat, float timeOffset = 0f)
    {
        int beatCount = BeatCount;
        if (beatCount > 0)
        {
            if (includeFinalBeat == false) beatCount -= 1;
            float[] beats = new float[beatCount];
            for (int b = 0; b < beatCount; b++) beats[b] = beatTimes[b] + timeOffset;
            return beats;
        }
        else return new float[0];
    }

    public int GetBeatIndex(float time)
    {
        if (BeatCount > 0)
        {
            // Find closest beat before time
            int getBeatIndex = Array.FindLastIndex(beatTimes, t => t <= time);
            if (getBeatIndex != -1)
            {
                // Beat is withing track range
                if (getBeatIndex < BeatCount - 1) return getBeatIndex;
                // Found beat is after the end of track: we need to extrapolate index
                else return BeatCount - 1 + Mathf.FloorToInt((time - beatTimes[BeatCount - 1]) / EndBeatDuration);
            }
            // Time is before first beat: extrapolate
            else
            {
                float floatingBeatIndex = (time - beatTimes[0]) / StartBeatDuration;
                return Mathf.FloorToInt(floatingBeatIndex);
            }
        }
        // Beat track is empty, return 0 as default index
        return 0;
    }

    public float GetBeatStartTime(int beatIndex)
    {
        if (BeatCount == 0) return 0f;
        // Beat is before track range
        if (beatIndex <= 0) return StartBeatDuration * beatIndex;
        // Beat is withing track range
        else if (beatIndex < BeatCount - 1) return beatTimes[beatIndex];
        // Beat is after track range
        else return beatTimes[BeatCount - 1] + EndBeatDuration * (beatIndex - BeatCount + 1);
    }

    public float GetBeatDuration(int beatIndex)
    {
        // Beat is before track range
        if (beatIndex <= 0) return StartBeatDuration;
        // Beat is withing track range
        else if (beatIndex < BeatCount - 1) return beatTimes[beatIndex + 1] - beatTimes[beatIndex];
        // Beat is after track range
        else return EndBeatDuration;
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
    public float[] GetBarTimes(bool includeFinalBar, float timeOffset = 0f)
    {
        int barCount = BarCount;
        if (barCount > 0)
        {
            if (includeFinalBar == false) barCount -= 1;
            float[] bars = new float[barCount];
            for (int b = 0; b < barCount; b++) bars[b] = barTimes[b] + timeOffset;
            return bars;
        }
        else return new float[0];
    }

    public int GetBarIndex(float time)
    {
        if (BarCount > 0)
        {
            // Find closest bar before time
            int getBarIndex = Array.FindLastIndex(barTimes, t => t <= time);
            if (getBarIndex != -1)
            {
                // Bar is withing track range
                if (getBarIndex < BarCount - 1) return getBarIndex;
                // Found bar is after the end of track: we need to extrapolate time and index
                else return BarCount - 1 + Mathf.FloorToInt((time - barTimes[BarCount - 1]) / EndBarDuration);
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
        if (BarCount == 0) return 0f;
        // Bar is before track range
        if (barIndex <= 0) return StartBarDuration * barIndex;
        // Bar is withing track range
        else if (barIndex < BarCount - 1) return barTimes[barIndex];
        // Bar is after track range
        else return barTimes[BarCount - 1] + EndBarDuration * (barIndex - BarCount + 1);
    }

    public float GetBarDuration(int barIndex)
    {
        // Bar is before track range
        if (barIndex <= 0) return StartBarDuration;
        // Bar is withing track range
        else if (barIndex < BarCount - 1) return barTimes[barIndex + 1] - barTimes[barIndex];
        // Bar is after track range
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