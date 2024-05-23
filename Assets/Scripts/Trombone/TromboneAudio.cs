using UnityEngine;
using UnityEngine.Audio;
using System;

[ExecuteAlways]
public class TromboneAudio : MonoBehaviour,
    ITromboneBlowInput, ITromboneSlideToneInput, ITrombonePressureToneInput
{
    [Flags] public enum UpdateMode { Regular = 1, Fixed = 2, Late = 4, Manual = 8 }

    [Header("Execution")]
    public bool showDebug;
    public UpdateMode update;
    [Header("Control")]
    public bool blow;
    public float slideTone;
    public float pressureTone;
    [Header("Sound")]
    public SamplerInstrument sampler;
    public float pitch = 1f;
    public bool setPitchFromMusicPlayer = true;
    [Header("Audio")]
    [Range(1, 16)] public int audioSourceCapacity = 1;
    public AudioMixerGroup mixerGroup;
    public float smoothInSpeed = 1000f;
    public float smoothOutSpeed = 100f;
    public float fullVolume = 1f;

    private AudioSource activeSource;
    private AudioSource[] audioSources;
    private bool activeBlow;
    private float activePressureTone;
    private MusicPlayer musicPlayer;

    public bool? Blow { set { if (value != null) blow = value.Value; } }
    public float? SlideTone { set { if (value != null) slideTone = value.Value; } }
    public float? PressureTone { set { if (value != null) pressureTone = value.Value; } }

    private void Awake()
    {
        SetAudioSources(audioSourceCapacity);
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
    }

    private void Update()
    {
        if (audioSources == null || audioSources.Length != audioSourceCapacity) SetAudioSources(audioSourceCapacity);
        if (update.HasFlag(UpdateMode.Regular)) UpdateAudio(Time.deltaTime);
        if (setPitchFromMusicPlayer && musicPlayer) pitch = musicPlayer.playingSpeed;
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying == false) return;
        if (update.HasFlag(UpdateMode.Fixed)) UpdateAudio(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (Application.isPlaying == false) return;
        if (update.HasFlag(UpdateMode.Late)) UpdateAudio(Time.deltaTime);
    }

    public void ClearInputs()
    {
        blow = false;
        slideTone = float.NaN;
        pressureTone = float.NaN;
    }

    public void UpdateAudio(float deltaTime)
    {
        // If possible and needed, switch audio source to avoid clipping
        if (blow != activeBlow || pressureTone != activePressureTone) SwitchAudioSource();
        // Update audio sources
        foreach (AudioSource source in audioSources)
        {
            // Update audio settings on all sources
            source.outputAudioMixerGroup = mixerGroup;
            // Make active source play note on blow
            if (blow == true && float.IsNaN(slideTone) == false && float.IsNaN(pressureTone) == false && source == activeSource)
            {
                if (sampler != null)
                {
                    // Smooth volume up
                    if (source.volume < fullVolume) source.volume += deltaTime * smoothInSpeed;   
                    // Play tone with right pitch, combining slide position and music playing speed
                    sampler.PlayTone(pressureTone, source, pitchByTones: -slideTone, pitchMultiplier:pitch);
                    //source.pitch = pitch * Mathf.Pow(SamplerInstrument.PerTonePitchMultiplier, -slideTone);
                }
            }
            else
            {
                // Smooth volume down on inactive sources
                if (source.volume > 0) source.volume -= deltaTime * smoothOutSpeed;
                else source.Stop();
            }
        }
    }

    public void UpdateAudio() => UpdateAudio(Time.deltaTime);

    private void SwitchAudioSource()
    {
        AudioSource availableSource = Array.Find(audioSources, s => s.isPlaying == false);
        if (availableSource != null) activeSource = availableSource;
        else if (showDebug) Debug.LogWarning("Failed to find an available audio source on " + this + ". Audio might clip.");
        activeBlow = blow;
        activePressureTone = pressureTone;
    }

    private void SetAudioSources(int count)
    {
        audioSourceCapacity = Mathf.Max(count, 0);
        audioSources = GetComponents<AudioSource>();
        int audioSourceCurrentCount = audioSources.Length;
        if (audioSourceCurrentCount == audioSourceCapacity) return;
        while (audioSourceCurrentCount < audioSourceCapacity)
        {
            gameObject.AddComponent<AudioSource>();
            audioSourceCurrentCount++;
        }
        while (audioSourceCurrentCount > audioSourceCapacity)
        {
            DestroyImmediate(GetComponent<AudioSource>());
            audioSourceCurrentCount--;
        }
        audioSources = GetComponents<AudioSource>();
    }
}