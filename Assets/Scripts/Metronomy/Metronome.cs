using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[ExecuteAlways]
public class Metronome : MonoBehaviour
{
    public enum TimeMode { Manual, FixedUpdate, FollowAudio, FollowPlayhead }

    public bool showDebug;
    [Header("Timing")]
    public TimeMode timeMode;
    public Playhead playhead;
    public float playTime = 0f;
    [Header("Rythm")]
    public TempoInfo[] tempos;
    public MeasureInfo[] measures;
    [Header("Audio")]
    public bool click = false;
    public int sampleFrequency = 48000;
    public AudioClip barClickSound;
    public AudioClip beatClickSound;
    public float clickSyncTolerance = .05f;
    [Header("Events")]
    public UnityEvent onBar;
    public UnityEvent<float, float> onBarProgress;
    public UnityEvent onBeat;
    public UnityEvent<float, float> onBeatProgress;

    private float[] beatTimesSeconds;
    private float[] barTimesSeconds;
    private Playhead activePlayhead;
    private AudioSource clickSource;

    public TempoInfo Tempo { get; private set; }
    public MeasureInfo Measure { get; private set; }
    public float BarProgress { get; private set; }
    public float BeatProgress { get; private set; }
    //public float BPM => 60f / Tempo.SecondsPerBeat;

    private AudioClip ClickTrack
    {
        get => clickSource != null ? clickSource.clip : null;
        set { if (clickSource != null) clickSource.clip = value; }
    }

    private void Awake()
    {
        clickSource = GetComponent<AudioSource>();
    }

    private void OnValidate()
    {
        Debug.Log("OnValidate");
        SetRythm(tempos, measures);
    }

    private void FixedUpdate()
    {
        switch (timeMode)
        {
            case TimeMode.Manual:
                SetPlayhead(null);
                break;
            case TimeMode.FollowPlayhead:
                SetPlayhead(playhead);
                break;
            case TimeMode.FixedUpdate:
                SetPlayhead(null);
                MoveTime(playTime + Time.fixedDeltaTime);
                break;
            case TimeMode.FollowAudio:
                SetPlayhead(null);
                MoveTime(clickSource.time);
                break;
        }
    }

    public void SetPlayhead(Playhead p)
    {
        // Set playhead listeners
        if (activePlayhead != p)
        {
            if (activePlayhead != null) activePlayhead.onMove.RemoveListener(MoveTime);
            if (p != null) p.onMove.AddListener(MoveTime);
            activePlayhead = p;
        }
    }    

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

    private void MoveTime(float toTime) => MoveTime(playTime, toTime);

    private void MoveTime(float fromTime, float toTime)
    {
        //// Read tempo infos
        //int tempoSectionIndex = tempoTrack.GetTempoSectionIndex(toTime);
        //Tempo = tempoTrack.GetTempo(tempoSectionIndex);
        //// Get bar and beat progress
        //BarProgress = tempoTrack.GetBarProgress(toTime, tempoSectionIndex);
        //BeatProgress = (BarProgress * Tempo.BeatsPerBar) % 1f;
        //// Get bars passed during this lap of time
        //int barCount = tempoTrack.CountBars(fromTime, toTime);
        //float oldBarProgress = BarProgress;
        //// Trigger events
        //if (fromTime != toTime)
        //{
        //    onBarProgress.Invoke(oldBarProgress, BarProgress);
        //    for (int i = 0; i < barCount; i++) onBar.Invoke();
        //}
        // Update playTime
        playTime = toTime;
    }

    private void GenerateClickTrack()
    {
        //float trackDurationSeconds = tempoTrack.LastSectionEnd - tempoTrack.FirstSectionStart;
        //if (trackDurationSeconds <= 0f)
        //{
        //    ClickTrack = null;
        //    return;
        //}
        //// Create empty click track
        //int trackDurationSamples = Mathf.CeilToInt(trackDurationSeconds * sampleFrequency);
        //float[] clickTrackSamples = new float[trackDurationSamples];
        //// Add bar sounds
        //float[] barTimes = tempoTrack.Bars;
        //if (barTimes != null && barClickSound != null)
        //{
        //    if (barClickSound.frequency != sampleFrequency) Debug.LogWarning("Bar click sample rate mismatch (" + barClickSound.frequency + "Hz).");
        //    // Sample bar sound
        //    float[] barClickSamples = new float[barClickSound.samples];
        //    barClickSound.GetData(barClickSamples, 0);
        //    // Add samples at bar times
        //    foreach (float t in barTimes) AudioSampling.AddTo(ref clickTrackSamples, barClickSamples, Mathf.CeilToInt(t * sampleFrequency));
        //}
        //// Add beat sounds

        //// Set audio clip
        //ClickTrack = AudioClip.Create("ClickTrack", trackDurationSamples, 1, sampleFrequency, false);
        //ClickTrack.SetData(clickTrackSamples, 0);
    }

    private void AudioClickPlayback()
    {
        //// Turn audio click off
        //if (click == false)
        //{
        //    clickSource.Stop();
        //    return;
        //}
        //// Check audio track
        //if (ClickTrack == null)
        //{
        //    Debug.LogWarning("Click track was not generated. Stopping audio click.");
        //    click = false;
        //    return;
        //}
        //// Loop audio so metronome doesn't stop on its own
        //clickSource.Play();
        //clickSource.loop = true;
        //// Loop click track before first tempo section and after last tempo section
        //if (playTime < tempoTrack.FirstSectionStart)
        //{
        //    int audioTimeSamples = clickSource.timeSamples;
        //    int firstSectionDurationSamples = (int)(tempoTrack.FirstSectionEnd - tempoTrack.FirstSectionStart) * sampleFrequency;
        //    if (audioTimeSamples >= firstSectionDurationSamples)
        //    {
        //        clickSource.timeSamples = (int)Mathf.Repeat(audioTimeSamples, firstSectionDurationSamples);
        //    }
        //}
        //else if (playTime > tempoTrack.LastSectionStart)
        //{
        //    int audioTimeSamples = clickSource.timeSamples;
        //    int lastSectionStartSamples = (int)(tempoTrack.LastSectionStart - tempoTrack.FirstSectionStart) * sampleFrequency;
        //    int lastSectionEndSamples = (int)(tempoTrack.LastSectionEnd - tempoTrack.FirstSectionStart) * sampleFrequency;
        //    if (audioTimeSamples < lastSectionStartSamples || audioTimeSamples >= lastSectionEndSamples)
        //    {
        //        int lastSectionDurationSamples = (int)(tempoTrack.LastSectionEnd - tempoTrack.LastSectionStart) * sampleFrequency;
        //        clickSource.timeSamples = (int)(lastSectionStartSamples + Mathf.Repeat(audioTimeSamples, lastSectionDurationSamples));
        //    }
        //}
    }
}
