using System;
using UnityEngine;
using UnityEngine.Events;

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
    public UnityEvent<float, float> onTimeChange;
    public UnityEvent<int,int> onBeatChange;
    public UnityEvent<int, int> onBarChange;

    private MetronomeTrack rythmTrack;
    private Playhead activePlayhead;
    private AudioSource clickSource;

    public BeatInfo CurrentBeat { get; private set; }
    public BarInfo CurrentBar { get; private set; }
    public float CurrentBeatProgress => CurrentBeat.duration != 0f ? (playTime - CurrentBeat.startTime) / CurrentBeat.duration : 0f;
    public float CurrentBarProgress => CurrentBar.durationInSeconds != 0f ? (playTime - CurrentBar.startTime) / CurrentBar.durationInSeconds : 0f;

    public AudioClip ClickTrack
    {
        get => clickSource != null ? clickSource.clip : null;
        private set { if (clickSource != null) clickSource.clip = value; }
    }

    private void Awake()
    {
        clickSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        CurrentBeat = rythmTrack.GetBeat(playTime);
        CurrentBar = rythmTrack.GetBar(playTime);
    }

    private void OnValidate()
    {
        rythmTrack.SetRythm(tempos, measures);
        MoveTime(playTime);
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
    
    public void MoveTime(float toTime) => MoveTime(playTime, toTime);

    private void MoveTime(float fromTime, float toTime)
    {
        // Update playTime
        playTime = toTime;
        // Update beat
        int beatIndex = CurrentBeat.index;
        CurrentBeat = rythmTrack.GetBeat(playTime);
        // Update bar
        int barIndex = CurrentBeat.index;
        CurrentBar = rythmTrack.GetBar(playTime);
        // Trigger events
        if (fromTime != toTime) onTimeChange.Invoke(playTime, toTime);
        if (CurrentBeat.index != beatIndex) onBeatChange.Invoke(beatIndex, CurrentBeat.index);
        if (CurrentBar.index != barIndex) onBarChange.Invoke(barIndex, CurrentBar.index);
    }

    private void GenerateClickTrack()
    {
        float trackDurationSeconds = rythmTrack.TrackDuration;
        // Create empty click track
        int trackDurationSamples = Mathf.CeilToInt(trackDurationSeconds * sampleFrequency);
        float[] clickTrackSamples = new float[trackDurationSamples];
        // Add bar sounds
        if (barClickSound != null)
        {
            float[] barClickSamples = new float[barClickSound.samples];
            barClickSound.GetData(barClickSamples, 0);
            barClickSamples = AudioSampling.StereoToMono(barClickSamples);
            foreach (float barTime in rythmTrack.barTimes)
            {
                int timeSamples = Mathf.FloorToInt(barTime * sampleFrequency);
                AudioSampling.AddTo(ref clickTrackSamples, barClickSamples, timeSamples);
            }
        }
        // Add beat sounds
        if (beatClickSound != null)
        {
            float[] beatClickSamples = new float[beatClickSound.samples];
            beatClickSound.GetData(beatClickSamples, 0);
            beatClickSamples = AudioSampling.StereoToMono(beatClickSamples);
            foreach (float beatTime in rythmTrack.beatTimes)
            {
                // Don't add beat sound on bar
                if (Array.IndexOf(rythmTrack.barTimes, beatTime) == -1)
                {
                    int timeSamples = Mathf.FloorToInt(beatTime * sampleFrequency);
                    AudioSampling.AddTo(ref clickTrackSamples, beatClickSamples, timeSamples);
                }
            }
        }
        // Set audio clip
        ClickTrack = AudioClip.Create("ClickTrack", trackDurationSamples, 1, sampleFrequency, false);
        ClickTrack.SetData(clickTrackSamples, 0);
    }

    private void AudioClickPlayback()
    {
        if (ClickTrack == null) return;
        // Turn audio click off
        if (click == false)
        {
            clickSource.Stop();
            return;
        }
        // Check audio track
        if (ClickTrack == null)
        {
            Debug.LogWarning("Click track was not generated. Stopping audio click.");
            click = false;
            return;
        }
        // Sync audio on playTime
        if (timeMode != TimeMode.FollowAudio)
        {
            //float audioTime;
            //// Pre-track loop
            //if (playTime < 0f)
            //{

            //}
            //// In track
            //else if (playTime < ClickTrack.length)
            //{
            //    audioTime = playTime;
            //}
            //// Post-track loop
            //else
            //{

            //}
            //// Sync (with tolerance)
            //if (Mathf.Abs(clickSource.time - audioTime) > clickSyncTolerance) clickSource.time = audioTime;
        }
        // Play audio
        if (clickSource.isPlaying == false) clickSource.Play();
    }
}
