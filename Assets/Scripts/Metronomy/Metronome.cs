using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class Metronome : MonoBehaviour
{
    public enum TimeMode { Manual, FixedUpdate, FollowAudio, FollowPlayhead }

    public bool showDebug;
    [SerializeField] private TempoInfo[] tempos;
    [SerializeField] private MeasureInfo[] measures;
    [Header("Timing")]
    public TimeMode timeMode;
    public Playhead playhead;
    public float playTime = 0f;
    public float playSpeed = 1f;
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
    private AudioClip audioClickMainClip;
    private AudioClip audioClickStartLoop;
    private AudioClip audioClickEndLoop;
    private float audioTime;

    public BeatInfo CurrentBeat { get; private set; }
    public BarInfo CurrentBar { get; private set; }
    public float CurrentBeatProgress => CurrentBeat.duration != 0f ? (playTime - CurrentBeat.startTime) / CurrentBeat.duration : 0f;
    public float CurrentBarProgress => CurrentBar.durationInSeconds != 0f ? (playTime - CurrentBar.startTime) / CurrentBar.durationInSeconds : 0f;

    private AudioClip AudioClick
    {
        get => clickSource != null ? clickSource.clip : null;
        set 
        {
            if (clickSource != null && clickSource.clip != value)
            {
                bool isPlaying = clickSource.isPlaying;
                clickSource.Stop();
                clickSource.clip = value;
                clickSource.time = 0f;
                if (isPlaying && value != null) clickSource.Play();
            }
        }
    }

    private void Awake()
    {
        clickSource = GetComponent<AudioSource>();
        SetRythm();
    }

    private void OnEnable()
    {
        CurrentBeat = rythmTrack.GetBeat(playTime);
        CurrentBar = rythmTrack.GetBar(playTime);
    }

    private void OnValidate()
    {
        if (rythmTrack.IsReady == false
            || tempos.SequenceEqual(rythmTrack.TempoChanges) == false 
            || measures.SequenceEqual(rythmTrack.MeasureChanges) == false)
            SetRythm();
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
                MoveTime(playTime + Time.fixedDeltaTime * playSpeed);
                break;
            case TimeMode.FollowAudio:
                SetPlayhead(null);
                MoveTime(clickSource.time);
                break;
        }
        AudioClickPlayback();
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

    public void MoveTime(float fromTime, float toTime)
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

    public TempoInfo[] TempoChanges
    {
        get => tempos != null ? Array.ConvertAll(tempos, t => t) : new TempoInfo[0];
        set
        {
            if (value != null)
            {
                if (tempos.SequenceEqual(value) == false)
                {
                    tempos = Array.ConvertAll(value, t => t);
                    SetRythm();
                }
            }
            else
            {
                tempos = new TempoInfo[0];
                SetRythm();
            }
        }
    }

    public MeasureInfo[] MeasureChanges
    {
        get => measures != null ? Array.ConvertAll(measures, t => t) : new MeasureInfo[0];
        set
        {
            if (value != null)
            {
                if (measures.SequenceEqual(value) == false)
                {
                    measures = Array.ConvertAll(value, m => m);
                    SetRythm();
                }
            }
            else
            {
                measures = new MeasureInfo[0];
                SetRythm();
            }
        }
    }

    public void SetRythm(TempoInfo[] tempoChanges, MeasureInfo[] measureChanges)
    {
        tempos = tempoChanges != null ? Array.ConvertAll(tempoChanges, t => t) : new TempoInfo[0];
        measures = measureChanges != null ? Array.ConvertAll(measureChanges, m => m) : new MeasureInfo[0];
        SetRythm();
    }

    private void SetRythm()
    {
        // Generate rythm track
        rythmTrack = new MetronomeTrack(tempos, measures);
        if (rythmTrack.IsReady == false)
        {
            audioClickStartLoop = null;
            audioClickMainClip = null;
            audioClickEndLoop = null;
            return;
        }
        // Generate audio track
        float[] beatClickSamples = AudioSampling.GetMonoSamples(beatClickSound);
        float[] barClickSamples = AudioSampling.GetMonoSamples(barClickSound);
        // Generate main click track (exclude final beat and bar)
        float[] beatTimes = rythmTrack.GetBeatTimes(false);
        float[] barTimes = rythmTrack.GetBarTimes(false);
        audioClickMainClip = GenerateClickTrack(rythmTrack.TrackDuration, beatTimes, barTimes, beatClickSamples, barClickSamples);
        audioClickMainClip.name = "mainClick";
        // Generate start loop (one measure at constant tempo)
        audioClickStartLoop = GenerateClickTrack(rythmTrack.StartBarDuration, rythmTrack.StartBeatDuration, rythmTrack.StartBarDuration, beatClickSamples, barClickSamples);
        audioClickStartLoop.name = "startClick";
        // Generate end loop (same with end tempo)
        audioClickEndLoop = GenerateClickTrack(rythmTrack.EndBarDuration, rythmTrack.EndBeatDuration, rythmTrack.EndBarDuration, beatClickSamples, barClickSamples);
        audioClickEndLoop.name = "endClick";
    }

    private AudioClip GenerateClickTrack(float duration, float beatDuration, float barDuration, float[] beatSamples, float[] barSamples, float beatOffset = 0f, float barOffset = 0f)
    {
        int beatCount = Mathf.FloorToInt((duration - beatOffset) / beatDuration);
        float[] beatTimes = new float[beatCount];
        for (int i = 0; i < beatCount; i++) beatTimes[i] = beatOffset + i * beatDuration;
        int barCount = Mathf.FloorToInt((duration - barOffset) / barDuration);
        float[] barTimes = new float[barCount];
        for (int i = 0; i < barCount; i++) barTimes[i] = barOffset + i * barDuration;
        return GenerateClickTrack(duration, beatTimes, barTimes, beatSamples, barSamples);
    }

    private AudioClip GenerateClickTrack(float duration, float[] beatTimes, float[] barTimes, float[] beatSamples, float[] barSamples)
    {
        int durationSamples = Mathf.CeilToInt(duration * sampleFrequency);
        float[] clickTrackSamples = new float[durationSamples];
        // Add beat sounds
        foreach (float beatTime in beatTimes)
        {
            // Don't add beat sound on bar
            if (Array.IndexOf(barTimes, beatTime) == -1)
            {
                int timeSamples = Mathf.FloorToInt(beatTime * sampleFrequency);
                AudioSampling.AddTo(ref clickTrackSamples, beatSamples, timeSamples);
            }
        }
        // Add bar sounds
        foreach (float barTime in barTimes)
        {
            int timeSamples = Mathf.FloorToInt(barTime * sampleFrequency);
            AudioSampling.AddTo(ref clickTrackSamples, barSamples, timeSamples);
        }
        // Set audio clip
        AudioClip audio = AudioClip.Create("", durationSamples, 1, sampleFrequency, false);
        audio.SetData(clickTrackSamples, 0);
        return audio;
    }

    private void AudioClickPlayback()
    {
        if (clickSource == null) return;
        // Turn audio click off
        if (click == false)
        {
            clickSource.Stop();
            return;
        }
        // Ensure audio was generated
        if (audioClickStartLoop == null || audioClickMainClip == null || audioClickEndLoop == null)
        {
            Debug.LogWarning("Metronome audio must be generated before click can be activated.");
            click = false;
            return;
        }
        // Sync audio and playtime
        if (timeMode == TimeMode.FollowAudio) SyncPlaytimeOnAudio();
        else SyncAudioOnPlaytime();
        // Play audio
        clickSource.pitch = playSpeed;
        if (clickSource.isPlaying == false) clickSource.Play();
    }

    private void SyncAudioOnPlaytime()
    {
        audioTime = playTime - rythmTrack.FirstBeatTime;
        // Pre-track loop
        if (audioTime < 0f)
        {
            audioTime = Mathf.Repeat(audioTime, audioClickStartLoop.length);
            AudioClick = audioClickStartLoop;
        }
        // In track
        else if (audioTime < audioClickMainClip.length)
        {
            AudioClick = audioClickMainClip;
        }
        // Post-track loop
        else
        {
            audioTime = Mathf.Repeat(audioTime - audioClickMainClip.length, audioClickEndLoop.length);
            AudioClick = audioClickEndLoop;
        }
        // Sync (with tolerance)
        if (AudioClick != null)
        {
            clickSource.loop = true;
            if (Mathf.Abs(clickSource.time - audioTime) > clickSyncTolerance)
            {
                // Take loop in consideration
                float loopLength = AudioClick.length;
                if (Mathf.Abs(clickSource.time - audioTime + loopLength) > clickSyncTolerance && Mathf.Abs(clickSource.time - audioTime - loopLength) > clickSyncTolerance)
                    // Force sync
                    clickSource.time = audioTime;
            }
        }
    }

    private void SyncPlaytimeOnAudio()
    {
        Debug.LogWarning("Audio sync not implemented");
    }
}
