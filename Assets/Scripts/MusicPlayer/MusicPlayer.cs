using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;

public class MusicPlayer : MonoBehaviour
{
    public enum PlayState { Stop, Pause, Play, Transition }

    [Header("Execution")]
    public bool showDebug = true;
    public bool playOnStart = false;
    public bool fixedUpdate = true;
    [Header("Music")]
    [SerializeField] private SheetMusic music;
    // public Orchestra orchestra;
    public float tempoModifier = 1f;
    public float keyModifier = 0f;
    [Header("Timing")]
    public Metronome metronome;
    public float playTime = 0f;
    public float playingSpeed = 1f;
    public float playStateTransitionDuration = .5f;
    public bool loop = false;
    [Header("Audio")]
    public AudioTrackGenerator audioGenerator;
    public AudioSource audioSource;
    public float audioSyncTolerance = .5f;
    [Header("Notes")]
    public Playhead[] playHeads;
    public SheetMusicVoiceIdentifier playheadPart = new("trombone");
    public PerformanceStyle playheadStyle = PerformanceStyle.Default;
    [Header("Events")]
    public UnityEvent onPlayerUpdate;
    public UnityEvent onMusicEnd;

    public SheetMusic LoadedMusic { get; private set; }
    public NoteInstance[] LoadedNotes { get; private set; }
    public AudioClip LoadedAudio => audioSource.clip;
    public PlayState PlayingState { get; private set; }
    public float CurrentPlayTime { get; private set; }
    public float MusicDuration => LoadedMusic != null ? LoadedMusic.GetDuration(/*TempoStretch*/) : 0f;
    public float CurrentPlayingSpeed { get; private set; }
    public float LoopedPlayTime { get; private set; }
    public bool IsReversePlaying { get; private set; }
    public int ConsecutiveAudioSyncs { get; private set; }
    public bool IsLoading => audioGenerator != null && audioGenerator.AudioIsReady == false;
    //public float TempoStretch => tempoModifier != 0f ? 1f / tempoModifier : 0f;

    private bool hasLooped;
    private float playHeadsTime;
    private float applicationDeltaTime;
    private float audioLag;

    private void Awake()
    {
        //LoadedMusic = null;
        CurrentPlayTime = playTime;
        LoopedPlayTime = playTime;
        CurrentPlayingSpeed = playingSpeed;
        ConsecutiveAudioSyncs = 0;
        audioLag = 0f;
        playHeadsTime = CurrentPlayTime;
    }

    private void Start()
    {
        LoadMusic(music);
        if (playOnStart) Play();
    }

    private void OnEnable()
    {
        audioGenerator?.onGenerationProgress?.AddListener(OnAudioGeneratorProgress);
    }

    private void OnDisable()
    {
        Stop();
        audioGenerator?.onGenerationProgress?.RemoveListener(OnAudioGeneratorProgress);
    }

    private void Update()
    {
        if (!fixedUpdate)
        {
            applicationDeltaTime = Time.deltaTime;
            PlayerUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (fixedUpdate)
        {
            applicationDeltaTime = Time.fixedDeltaTime;
            PlayerUpdate();
        }
    }

    public void LoadMusic()
    {
        if (showDebug) Debug.Log("Loading Music " + music?.name);
        // Make a copy of the sheet music to avoid modifying the original
        LoadedMusic = music != null ? Instantiate(music) : ScriptableObject.CreateInstance<SheetMusic>();
        LoadedMusic.name = music != null ? music.name + " (copy)" : "";
        // Alter sheet music
        LoadedMusic.MultiplyTempoBy(tempoModifier);
        LoadedMusic.TransposeBy(keyModifier);
        // Load notes read by playheads
        NoteInfo[] loadedNoteInfos = LoadedMusic.GetPartNotes(playheadPart);
        playheadStyle?.ProcessNotes(ref loadedNoteInfos);
        LoadedNotes = Array.ConvertAll(loadedNoteInfos, n => new NoteInstance(n));
        // Generate audio
        if (audioGenerator)
        {
            audioGenerator.music = LoadedMusic;
            audioGenerator.mutedParts = new SheetMusicVoiceIdentifier[1] { playheadPart };
            audioGenerator.SampleTrack();
            StartCoroutine(WaitForGeneratedAudio());
        }
        // Metronome setup
        if (metronome)
        {
            metronome.playSpeed = playingSpeed;
            metronome.SetRythm(LoadedMusic);
        }
    }

    public void LoadMusic(SheetMusic sheetMusic, AudioClip preGeneratedAudio = null, SamplerInstrument playedInstrument = null, int voiceIndex = 1)
    {
        music = sheetMusic;
        playheadPart = new(playedInstrument?.instrumentName, true, voiceIndex);
        if(NeedsReload()) LoadMusic();
    }

    public bool IsLoadedMusic(SheetMusic sheetMusic)
    {
        if (LoadedMusic == null) return sheetMusic != null;
        if (sheetMusic == null) return LoadedMusic != null;
        return LoadedMusic.MusicEquals(sheetMusic, tempoModifier, keyModifier) == false;
    }

    public bool NeedsReload() => IsLoadedMusic(music);

    private void OnAudioGeneratorProgress(float progress)
    {
        if (audioSource == null || audioGenerator == null) return;
        if (progress >= 1f) audioSource.clip = audioGenerator.generatedAudio;
    }

    private IEnumerator WaitForGeneratedAudio()
    {
        audioSource.clip = null;
        yield return new WaitUntil(() => audioGenerator.AudioIsReady);
        audioSource.clip = audioGenerator.generatedAudio;
    }

    public void UnloadMusic()
    {
        StartCoroutine(UnloadAudioCoroutine(audioSource.clip));
    }

    private IEnumerator UnloadAudioCoroutine(AudioClip audio)
    {
        // Wait until audio is fully loaded, then unload it
        if (audio != null)
        {
            if (showDebug) Debug.Log("Unloading backing track: " + audio.name + " ...");
            yield return new WaitWhile(() => audio.loadState == AudioDataLoadState.Loading);
            audio.UnloadAudioData();
            yield return new WaitUntil(() => audio.loadState == AudioDataLoadState.Unloaded);
            if (showDebug) Debug.Log("...unloading complete: " + audio.name);
        }
    }

    //public void ReloadMusic(string playedInstrument = null, int voiceIndex = 1)
    //{
    //    UnloadMusic();
    //    LoadMusic(music, null, playedInstrument, voiceIndex);
    //}

    private void PlayerUpdate()
    {
        // Check load state
        if (LoadedMusic == null)
        {
            if (PlayingState != PlayState.Stop) Stop();
            return;
        }
        // Loading audio
        if (IsLoading)
        {
            if (showDebug)
            {
                AudioClip audio = audioGenerator.generatedAudio;
                if (showDebug) Debug.Log("Loading audio...(" + audio + ") : " + (int)(audioGenerator.GenerationProgress * 100f) + "%");
            }
        }
        // Update play state
        else
        {
            UpdateAudio();
            UpdatePlayheads();
            UpdatePlayTime();
            if (showDebug && hasLooped) Debug.Log("Music has looped at " + CurrentPlayTime + "s back to " + LoopedPlayTime + "s");            
        }
        onPlayerUpdate.Invoke();
    }

    private void UpdatePlayTime()
    {
        if (PlayingState == PlayState.Stop)
        {
            playTime = 0f;
            CurrentPlayTime = 0f;
            LoopedPlayTime = 0f;
            CurrentPlayingSpeed = 0f;
            LoopedPlayTime = 0f;
            IsReversePlaying = false;
            audioLag = 0f;
            ConsecutiveAudioSyncs = 0;
        }
        // Manually changing playtime
        bool manualPlaytimeSet = playTime != CurrentPlayTime;
        if (manualPlaytimeSet)
        {
            // Artificially set play speed
            CurrentPlayingSpeed = (playTime - CurrentPlayTime) / applicationDeltaTime;
            CurrentPlayTime = playTime;
        }
        // Regular time update
        if (PlayingState == PlayState.Play || PlayingState == PlayState.Transition)
        {
            // Set playing speed
            if (manualPlaytimeSet == false && PlayingState != PlayState.Transition) CurrentPlayingSpeed = playingSpeed;
            if (CurrentPlayingSpeed != 0f)
            {
                IsReversePlaying = CurrentPlayingSpeed < 0f;
                // Move playtime
                float deltaPlayTime = CurrentPlayingSpeed * applicationDeltaTime;
                CurrentPlayTime += deltaPlayTime;
                playTime = CurrentPlayTime;
                LoopedPlayTime += deltaPlayTime;
                float musicDuration = LoadedMusic != null ? LoadedMusic.GetDuration(/*TempoStretch*/) : 0f;
                // Out of music
                if ((!IsReversePlaying && LoopedPlayTime > musicDuration)
                || (IsReversePlaying && LoopedPlayTime < 0f))
                {
                    if (loop)
                    {
                        LoopedPlayTime = Mathf.Repeat(CurrentPlayTime, musicDuration);
                        hasLooped = true;
                    }
                    else
                    {
                        onMusicEnd.Invoke();
                    }
                }
                else
                {
                    LoopedPlayTime = Mathf.Repeat(CurrentPlayTime, musicDuration);
                    hasLooped = false;
                }
            }
        }
    }

    private void UpdateAudio()
    {
        // Check audiosource
        if (audioSource == null || audioSource.enabled == false) return;
        // Set audio track and source
        float audioLength = audioSource.clip != null ? audioSource.clip.length : 0f;
        audioSource.loop = loop;
        // Playing
        if (PlayingState == PlayState.Play || PlayingState == PlayState.Transition)
        {
            // Pre-audio
            if (LoopedPlayTime < 0f)
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
            // Playing audio
            else if (LoopedPlayTime < audioLength)
            {
                // Sync audio and playtime
                if (audioSource.isPlaying)
                {
                    // When playing, sync audio and playtime
                    float audioTime = Mathf.Repeat(audioSource.time, audioSource.clip.length);
                    audioLag = LoopedPlayTime - audioTime;
                    if (Mathf.Abs(audioLag) > Mathf.Abs(audioSyncTolerance * CurrentPlayingSpeed))
                    {
                        if (showDebug) Debug.LogWarning("Audio sync at " + LoopedPlayTime + " by " + audioLag);
                        audioSource.time = LoopedPlayTime;
                        // Watch out for lag (consecutive resyncs)
                        ++ConsecutiveAudioSyncs;
                    }
                    else
                        ConsecutiveAudioSyncs = 0;
                }
                // Speed
                if (CurrentPlayingSpeed == 0f)
                    audioSource.Pause();
                else
                {
                    audioSource.pitch = CurrentPlayingSpeed;
                    // Check if backing source is playing
                    if (audioSource.isPlaying == false && audioSource.clip != null
                        && audioSource.time >= 0f && audioSource.time < audioSource.clip.length)
                    {
                        // If not, restart backing source
                        audioSource.Stop();
                        audioSource.time = LoopedPlayTime;
                        audioSource.Play();
                        if (showDebug) Debug.Log("Restarting backing source (" + audioSource.time + "s)");
                    }
                }
            }
            // Post-audio
            else
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }
        // Not Playing
        else 
        {
            if (audioSource.isPlaying)
            {
                if (PlayingState == PlayState.Pause)
                    audioSource.Pause();
                if (PlayingState == PlayState.Stop)
                    audioSource.Stop();
            }
        }
        // Metronome speed
        if (metronome) metronome.playSpeed = playingSpeed;
    }

    private void UpdatePlayheads()
    {
        if (PlayingState != PlayState.Stop) MovePlayheads(playHeadsTime, CurrentPlayTime);
    }

    private void MovePlayheads(float fromTime, float toTime, bool offsetFromTime = true, bool offsetToTime = true)
    {
        // Make playhead read notes
        if (playHeads != null && LoadedMusic != null)
        {
            float musicDuration = LoadedMusic != null ? LoadedMusic.GetDuration(/*TempoStretch*/) : 0f;
            foreach (Playhead p in playHeads)
            {
                if (p == null) continue;
                //if (hasLooped) p.loop = loop;
                p.loop = loop;
                p.loopStart = 0f;
                p.loopEnd = musicDuration;
                if (p is Playhead<INoteInfo>) (p as Playhead<INoteInfo>).Move(LoadedNotes, fromTime, toTime, offsetFromTime, offsetToTime);
            }
        }
        // Update playheads time
        playHeadsTime = toTime;
    }

    private void ClearPlayheads()
    {
        if (playHeads == null) return;
        foreach (Playhead p in playHeads)
            if (p != null) p.Clear();
    }

    

    public void SetPlaytimeSamples(int timeSamples)
    {
        audioSource.timeSamples = timeSamples;
        playTime = audioSource.time;
    }

    public void Play(bool speedUpEffect = false)
    {
        if (showDebug) Debug.Log("Play music");
        // Play from stop state
        if (PlayingState == PlayState.Stop)
        {
            MovePlayheads(0f, 0f);//, false, true);
        }
        // Play from any state
        if (speedUpEffect) Play(playStateTransitionDuration);
        else
        {
            CurrentPlayingSpeed = playingSpeed;
            PlayingState = PlayState.Play;
        }
    }

    public void Play(float speedUpEffect)
    {
        if (speedUpEffect > 0f) StartCoroutine(SpeedUpToPlay(speedUpEffect));
    }

    public void Stop(bool speedDownEffect = false)
    {
        if (showDebug) Debug.Log("Stop music");
        if (speedDownEffect)
            Stop(playStateTransitionDuration);
        else
        {
            PlayingState = PlayState.Stop;
            CurrentPlayTime = 0f;
            LoopedPlayTime = 0f;
            playTime = 0f;
            audioSource.Stop();
            if (playHeads != null)
                foreach (Playhead p in playHeads)
                    p.Stop();
        }
    }

    public void Stop(float slowDownEffect)
    {
        if (slowDownEffect > 0f) StartCoroutine(SlowDownToStop(slowDownEffect));
    }

    public void Pause(bool speedDownEffect = false)
    {
        if (showDebug) Debug.Log("Pause music");
        if (speedDownEffect)
            Pause(playStateTransitionDuration);
        else
        {
            PlayingState = PlayState.Pause;
            audioSource.Pause();
            CurrentPlayingSpeed = 0f;
        }
    }

    public void Pause(float slowDownEffect)
    {
        if (slowDownEffect > 0f) StartCoroutine(SlowDownToPause(slowDownEffect));
    }

    public void PauseFor(float seconds)
    {
        if (seconds > 0f) StartCoroutine(PauseForSeconds(seconds));
    }

    public IEnumerator SpeedUpToPlay(float effectDuration)
    {
        float startSpeed = CurrentPlayingSpeed;
        Play();
        CurrentPlayingSpeed = startSpeed;
        PlayingState = PlayState.Transition;
        float effectSpeed = playingSpeed / effectDuration;
        while (CurrentPlayingSpeed < playingSpeed)
        {
            if (effectSpeed > 0f)
                CurrentPlayingSpeed = Mathf.MoveTowards(CurrentPlayingSpeed, playingSpeed, applicationDeltaTime * effectSpeed);
            yield return null;
        }
        PlayingState = PlayState.Play;
    }

    public IEnumerator SlowDownToStop(float effectDuration)
    {
        PlayingState = PlayState.Transition;
        float effectSpeed = CurrentPlayingSpeed / effectDuration;
        while (CurrentPlayingSpeed > 0f)
        {
            if (effectSpeed > 0f)
                CurrentPlayingSpeed = Mathf.MoveTowards(CurrentPlayingSpeed, 0f, applicationDeltaTime * effectSpeed);
            yield return null;
        }
        Stop();
    }

    public IEnumerator SlowDownToPause(float effectDuration)
    {
        PlayingState = PlayState.Transition;
        float effectSpeed = CurrentPlayingSpeed / effectDuration;
        while (CurrentPlayingSpeed > 0f)
        {
            if (effectSpeed > 0f)
                CurrentPlayingSpeed = Mathf.MoveTowards(CurrentPlayingSpeed, 0f, applicationDeltaTime * effectSpeed);
            yield return null;
        }
        Pause();
    }

    public IEnumerator PauseForSeconds(float seconds)
    {
        Pause();
        yield return new WaitForSeconds(seconds);
        Play();
    }


    // A bouger ?
    public void ApplySettings(GameSettingsInfo settings)
    {
        SetBackingVolume(settings.backingVolume);
        //displayLatency = settings.latencyCorrection;
        SetTouchControl(settings.useTouchControls);
    }

    public void SetBackingVolume(float volume)
    {
        if (audioSource != null) audioSource.volume = Mathf.Clamp01(volume);
    }

    public void SetTouchControl(bool useTouch)
    {
        //if (Trombone != null) Trombone.SetTouchControl(useTouch);
    }
}
