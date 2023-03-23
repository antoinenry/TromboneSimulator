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
