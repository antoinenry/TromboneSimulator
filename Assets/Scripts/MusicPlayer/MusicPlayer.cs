using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MusicPlayer : MonoBehaviour
{
    public enum PlayState { Stop, Pause, Play, Transition }

    [Header("Execution")]
    public bool showDebug = true;
    public bool playOnStart = false;
    public bool fixedUpdate = true;
    [Header("Music")]
    public SheetMusic music;
    public Orchestra orchestra;
    public InstrumentDictionary instrumentDictionary;
    [Header("Timing")]
    public Metronome metronome;
    public float playTime = 0f;
    public float playingSpeed = 1f;
    public float playStateTransitionDuration = .5f;
    public bool loop = false;
    [Header("Audio")]
    public AudioTrackGenerator backingGenerator;
    public AudioSource backingSource;
    public float audioSyncTolerance = .5f;
    [Header("Notes")]
    public Playhead[] playHeads;
    [Header("Events")]
    public UnityEvent onMusicLoad;
    public UnityEvent onMusicEnd;
    public UnityEvent onMusicStop;

    public SheetMusic LoadedMusic { get; private set; }
    public AudioClip LoadedAudio => backingSource.clip;
    public PlayState Playing { get; private set; }
    public float CurrentPlayTime { get; private set; }
    public float MusicDuration => LoadedMusic != null ? LoadedMusic.DurationSeconds : 0f;
    public float CurrentPlayingSpeed { get; private set; }
    public float LoopedPlayTime { get; private set; }
    public bool IsReversePlaying { get; private set; }
    public int ConsecutiveAudioSyncs { get; private set; }
    public bool IsLoading => backingGenerator != null && backingGenerator.AudioIsReady == false;

    private NotePlay[] loadedNotes;
    private bool hasLooped;
    private float playHeadsTime;
    private float applicationDeltaTime;
    private float audioLag;

    private void Awake()
    {
        LoadedMusic = null;
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

    private void OnDisable()
    {
        Stop();
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

    private void PlayerUpdate()
    {
        // Check load state
        if (LoadedMusic != music) LoadMusic(music);
        if (LoadedMusic == null)
        {
            if (Playing != PlayState.Stop) Stop();
            return;
        }
        // Loading audio
        if (IsLoading)
        {
            if (showDebug)
            {
                AudioClip audio = backingGenerator.generatedAudio;
                if (showDebug) Debug.Log("Loading audio...(" + audio + ") : " + (int)(backingGenerator.GenerationProgress * 100f) + "%");
            }
        }
        // Update play state
        else
        {
            UpdatePlayTime();
            if (showDebug && hasLooped) Debug.Log("Music has looped at " + CurrentPlayTime + "s back to " + LoopedPlayTime + "s");
            UpdateAudio();
            UpdatePlayheads();
        }
    }

    private void UpdatePlayTime()
    {
        if (Playing == PlayState.Stop)
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
        if (Playing == PlayState.Play || Playing == PlayState.Transition)
        {
            // Set playing speed
            if (manualPlaytimeSet == false && Playing != PlayState.Transition) CurrentPlayingSpeed = playingSpeed;
            if (CurrentPlayingSpeed != 0f)
            {
                IsReversePlaying = CurrentPlayingSpeed < 0f;
                // Move playtime
                float deltaPlayTime = CurrentPlayingSpeed * applicationDeltaTime;
                CurrentPlayTime += deltaPlayTime;
                playTime = CurrentPlayTime;
                LoopedPlayTime += deltaPlayTime;
                float musicDuration = LoadedMusic != null ? LoadedMusic.DurationSeconds : 0f;
                // Out of music
                if ((!IsReversePlaying && LoopedPlayTime > musicDuration)
                || (IsReversePlaying && LoopedPlayTime < 0f))
                {
                    if (!loop)
                    {
                        Stop();
                        onMusicEnd.Invoke();
                    }
                    else
                    {
                        LoopedPlayTime = Mathf.Repeat(CurrentPlayTime, musicDuration);
                        hasLooped = true;
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
        if (backingSource == null || backingSource.enabled == false) return;
        float audioLength = backingSource.clip != null ? backingSource.clip.length : 0f;
        // Playing
        if (Playing == PlayState.Play || Playing == PlayState.Transition)
        {
            // Pre-audio
            if (LoopedPlayTime < 0f)
            {
                if (backingSource.isPlaying) backingSource.Stop();
            }
            // Playing audio
            else if (LoopedPlayTime < audioLength)
            {
                // Sync audio and playtime
                if (backingSource.isPlaying)
                {
                    // Resync audio on loop
                    if (hasLooped)
                    {
                        backingSource.time = LoopedPlayTime;
                    }
                    // When playing, sync audio and watch out for lag
                    else
                    {
                        float audioTime = backingSource.time;
                        audioLag = LoopedPlayTime - audioTime;
                        if (Mathf.Abs(audioLag) > Mathf.Abs(audioSyncTolerance * CurrentPlayingSpeed))
                        {
                            if (showDebug) Debug.LogWarning("Audio sync at " + LoopedPlayTime + " by " + audioLag);
                            backingSource.time = LoopedPlayTime;
                            ++ConsecutiveAudioSyncs;
                        }
                        else
                            ConsecutiveAudioSyncs = 0;
                    }                    
                }
                // Speed
                if (CurrentPlayingSpeed == 0f)
                    backingSource.Pause();
                else
                {
                    backingSource.pitch = CurrentPlayingSpeed;
                    // Check if backing source is playing
                    if (backingSource.isPlaying == false && backingSource.clip != null
                        && backingSource.time >= 0f && backingSource.time < backingSource.clip.length)
                    {
                        // If not, restart backing source
                        backingSource.Stop();
                        backingSource.time = LoopedPlayTime;
                        backingSource.Play();
                    }
                }
            }
            // Post-audio
            else
            {
                if (backingSource.isPlaying) backingSource.Stop();
            }
        }
        // Not Playing
        else 
        {
            if (backingSource.isPlaying)
            {
                if (Playing == PlayState.Pause)
                    backingSource.Pause();
                if (Playing == PlayState.Stop)
                    backingSource.Stop();
            }
        }
    }

    private void UpdatePlayheads()
    {
        if (Playing != PlayState.Stop) MovePlayheads(playHeadsTime, CurrentPlayTime);
    }

    private void MovePlayheads(float fromTime, float toTime, bool offsetFromTime = true, bool offsetToTime = true)
    {
        // Make playhead read notes
        if (playHeads != null && LoadedMusic != null)
        {
            float musicDuration = LoadedMusic != null ? LoadedMusic.DurationSeconds : 0f;
            foreach (Playhead p in playHeads)
            {
                if (p != null)
                {
                    //if (hasLooped) p.loop = loop;
                    p.loop = loop;
                    p.loopStart = 0f;
                    p.loopEnd = musicDuration;
                    p.Move(loadedNotes, fromTime, toTime, offsetFromTime, offsetToTime);
                }
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

    public void LoadMusic(SheetMusic sheetMusic, AudioClip preloadedAudio = null, SamplerInstrument playedInstrument = null, int voiceIndex = 0)
    {
        // Unload previous music
        if (LoadedAudio != preloadedAudio) UnloadMusic();
        // Load new music
        LoadedMusic = sheetMusic;
        music = sheetMusic;
        if (LoadedMusic != null)
        {
            // Get notes to play
            loadedNotes = music.GetNotes(playedInstrument, voiceIndex);
            if (backingSource != null)
            {
                backingSource.clip = null;
                // If audio is preloaded, set as backing clip
                if (preloadedAudio != null) backingGenerator.generatedAudio = preloadedAudio;
                // Else generate new audio
                else
                {
                    if (backingGenerator != null)
                    {
                        backingGenerator.orchestra = orchestra;
                        backingGenerator.trackInfo = LoadedMusic;
                        backingGenerator.ignoredParts = null;
                        // Don't generate playable voices (main voice and played alternative)
                        if (playedInstrument != null) backingGenerator.IgnoreVoice(playedInstrument.instrumentName, true, voiceIndex);
                        backingGenerator.SampleTrack();
                    }
                }
                backingSource.clip = backingGenerator.generatedAudio;
            }
            // Metronome setup
            metronome.SetRythm(music.tempo, music.measure);
            // Event
            onMusicLoad.Invoke();
        }
        else
        {
            backingSource.clip = null;
        }
    }

    public void UnloadMusic()
    {
        StartCoroutine(UnloadAudioCoroutine(backingSource.clip));
    }

    public IEnumerator UnloadAudioCoroutine(AudioClip audio)
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

    public void SetPlaytimeSamples(int timeSamples)
    {
        backingSource.timeSamples = timeSamples;
        playTime = backingSource.time;
    }

    public void Play(bool speedUpEffect = false)
    {
        if (showDebug) Debug.Log("Play music");
        // Play from stop state
        if (Playing == PlayState.Stop)
        {
            MovePlayheads(0f, 0f, false, true);
        }
        // Play from any state
        if (speedUpEffect) Play(playStateTransitionDuration);
        else
        {
            CurrentPlayingSpeed = playingSpeed;
            Playing = PlayState.Play;
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
            //onMusicStop.Invoke();
            Playing = PlayState.Stop;
            CurrentPlayTime = 0f;
            LoopedPlayTime = 0f;
            playTime = 0f;
            backingSource.Stop();
            if (playHeads != null)
                foreach (Playhead p in playHeads)
                    p.Stop();
            onMusicStop.Invoke();
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
            Playing = PlayState.Pause;
            backingSource.Pause();
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
        Playing = PlayState.Transition;
        float effectSpeed = playingSpeed / effectDuration;
        while (CurrentPlayingSpeed < playingSpeed)
        {
            if (effectSpeed > 0f)
                CurrentPlayingSpeed = Mathf.MoveTowards(CurrentPlayingSpeed, playingSpeed, applicationDeltaTime * effectSpeed);
            yield return null;
        }
        Playing = PlayState.Play;
    }

    public IEnumerator SlowDownToStop(float effectDuration)
    {
        Playing = PlayState.Transition;
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
        Playing = PlayState.Transition;
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
        if (backingSource != null) backingSource.volume = Mathf.Clamp01(volume);
    }

    public void SetTouchControl(bool useTouch)
    {
        //if (Trombone != null) Trombone.SetTouchControl(useTouch);
    }
}
