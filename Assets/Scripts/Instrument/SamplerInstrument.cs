using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewInstrument", menuName = "Trombone Hero/Instruments/Instrument")]
public class SamplerInstrument : ScriptableObject
{
    [Serializable]
    public struct ToneInfo
    {
        public string name;
        [Tone] public float tone;
        public int audioStartSamples;
        public int audioDurationSamples;
        public int attackSamples;
        public bool letRing;

        public int AudioEndSamples => audioStartSamples + audioDurationSamples;
    }

    public bool showDebug;
    [Header("Info")]
    public string instrumentName;
    public bool drumkit;
    [Header("Tone")]
    public AudioClip fullAudio;
    public AudioClip audioWithoutAttacks;
    public ToneInfo[] tones;
    public bool allowPitching = true;
    [Header("Performance")]
    public PerformanceStyle style;

    public int AudioChannels => fullAudio != null ? fullAudio.channels : 0;
    public float AudioFrequency => fullAudio != null ? fullAudio.frequency :0f;

    static public float PerTonePitchMultiplier => Mathf.Pow(2f, 1f / 12f);

    public float LowestTone
    {
        get
        {
            if (tones != null && tones.Length > 0)
            {
                float lowestTone = tones[0].tone;
                foreach (ToneInfo t in tones) lowestTone = Mathf.Min(t.tone, lowestTone);
                return lowestTone;
            }
            else
                return float.NaN;
        }
    }

    public float HighestTone
    {
        get
        {
            if (tones != null && tones.Length > 0)
            {
                float highestTone = tones[0].tone;
                foreach (ToneInfo t in tones) highestTone = Mathf.Max(t.tone, highestTone);
                return highestTone;
            }
            else
                return float.NaN;
        }
    }

    private void Awake()
    {
        if (style == null) style = PerformanceStyle.Default;
    }

    public void Load(int smoothInSamples, int smoothOutSamples)
    {
        if (fullAudio != null)
        {
            fullAudio.LoadAudioData();
            CreateAudioWithoutAttacks(smoothInSamples, smoothOutSamples);
        }
    }

    public void Load(float smoothInSpeed, float smoothOutSpeed)
    {
        int smoothInSamples = smoothInSpeed > 0f ? (int)(AudioFrequency / smoothInSpeed) : 0;
        int smoothOutSamples = smoothOutSpeed > 0f ? (int)(AudioFrequency / smoothOutSpeed) : 0;
        Load(smoothInSamples, smoothOutSamples);
    }

    public void Unload()
    {
        if (fullAudio != null)
        {
            fullAudio.UnloadAudioData();
        }
    }

    public void CreateAudioWithoutAttacks(int smoothInSamples, int smoothOutSamples)
    {
        // Get audio data
        int sampleLength = fullAudio.samples;
        int channels = fullAudio.channels;
        int frequency = fullAudio.frequency;
        float[] samples = new float[channels * sampleLength];
        fullAudio.GetData(samples, 0);
        // Process each tone audio
        foreach (ToneInfo tone in tones)
        {
            // Remove attack
            for (int s = 0; s < tone.attackSamples; s++)
                samples[tone.audioStartSamples + s] = 0f;
            // Smooth in
            AudioSampling.FadeIn(ref samples, channels, smoothInSamples, tone.audioStartSamples + tone.attackSamples);
            // Smooth out
            //AudioSampling.FadeOut(ref samples, channels, smoothOutSamples, tone.audioStartSamples + tone.audioDurationSamples);
        }
        // Set audio data
        audioWithoutAttacks = AudioClip.Create(fullAudio.name + "_noAttacks", sampleLength, channels, frequency, false);
        audioWithoutAttacks.SetData(samples, 0);
    }

    public void SetTones(float lowTone, float highTone)
    {
        SetTones(lowTone, highTone, Mathf.RoundToInt(highTone - lowTone + 1));
    }

    public void SetTones(float lowTone, float highTone, int toneCount)
    {
        if (toneCount < 0) toneCount = 0;
        tones = new ToneInfo[toneCount];
        float toneStep = (highTone + 1 - lowTone) / toneCount;
        float sampleCutLength = fullAudio != null ? (float)fullAudio.samples / toneCount : 0f;
        for (int t = 0; t < toneCount; t++)
        {
            tones[t] = new ToneInfo()
            {
                tone = lowTone + t * toneStep,
                name = ToneAttribute.GetNoteName(lowTone + t * toneStep),
                audioStartSamples = Mathf.FloorToInt(sampleCutLength * t),
                audioDurationSamples = Mathf.FloorToInt(sampleCutLength)
            };
        }
    }

    public bool TrySampleNote(NoteInfo note, bool removeAttack, out float[] samples, out int attackSamples)
    {
        if (tones != null && note.duration > 0f)
        {
            int toneIndex = -1;
            // For drumkits, one sound can be triggered by several alternative tones
            if (drumkit)
            {
                InstrumentDictionary.FindCurrentAlternativeDrumHitTones(note.tone, out float[] altenativeTones);
                if (altenativeTones != null) toneIndex = Array.FindIndex(tones, t => Array.IndexOf(altenativeTones, t.tone) != -1);
            }
            else
            {
                toneIndex = Array.FindIndex(tones, t => t.tone == note.tone);
            }
            // If tone is missing, try to find closest tone to pitch
            if (toneIndex == -1 && allowPitching) toneIndex = FindClosestToneIndex(note.tone);
            // Sample tone audio
            if (toneIndex != -1)
            {
                ToneInfo toneInfo = tones[toneIndex];
                float tonePitch = note.tone - toneInfo.tone;
                float noteDuration = Mathf.Max(note.duration, style.minimumNoteDuration);
                // Keep/Remove release
                int sampleLength = toneInfo.letRing ? 
                    toneInfo.audioDurationSamples: 
                    Mathf.CeilToInt(noteDuration * AudioFrequency) + toneInfo.attackSamples;
                // Keep/Remove attack
                int startSamples;
                if (removeAttack)
                {
                    sampleLength -= toneInfo.attackSamples;
                    startSamples = toneInfo.audioStartSamples + toneInfo.attackSamples;
                    attackSamples = 0;
                }
                else
                {
                    startSamples = toneInfo.audioStartSamples;
                    attackSamples = toneInfo.attackSamples;
                }
                // Get audio data
                samples = new float[AudioChannels * sampleLength];
                fullAudio.GetData(samples, startSamples);
                // Apply velocity
                samples = Array.ConvertAll(samples, d => d * style.velocityCurve.Evaluate(note.velocity));
                // Pitch audio
                if (tonePitch != 0f) samples = PitchTone(samples, AudioChannels, tonePitch);
                return true;
            }
        }
        samples = null;
        attackSamples = 0;
        return false;
    }

    public bool TryParseFileName(string fileName, out float lowTone, out float highTone)
    {
        lowTone = float.NaN;
        highTone = float.NaN;
        if (fileName == null || InstrumentDictionary.Current == null) return false;
        InstrumentDictionary.Current.FindOfficalName(fileName, out instrumentName);
        string[] noteNames = ToneAttribute.GetAllNoteNames();        
        foreach (string n in noteNames)
        {
            if (fileName.Contains(n))
            {
                float tone = ToneAttribute.GetNoteTone(n);
                if (float.IsNaN(lowTone))
                    lowTone = tone;
                else
                {
                    if (lowTone > tone)
                    {
                        highTone = lowTone;
                        lowTone = tone;
                    }
                    else
                        highTone = tone;
                    break;
                }
            }
        }
        return (lowTone != float.NaN && highTone != float.NaN);
    }

    public int TryGuessToneAttack(float[] samples, float threshold)
    {
        if (samples == null) return 0;
        int sampleLength = samples.Length;
        int peakIndex = -1;
        // Find first sample that is higher than threshold 
        peakIndex = Array.FindIndex(samples, s => Math.Abs(s) >= threshold);
        return peakIndex;
    }

    public void GuessToneAttacks(float threshold)
    {
        if (tones == null) return;
        int toneCount = tones.Length;
        for (int t = 0; t < toneCount; t++)
        {
            ToneInfo tone = tones[t];
            float[] samples = new float[tone.audioDurationSamples];
            fullAudio.GetData(samples, tone.audioStartSamples);
            tone.attackSamples = TryGuessToneAttack(samples, threshold);
            if (tone.attackSamples != -1) tones[t] = tone;
        }
    }

    public void PlayTone(float tone, AudioSource audioSource, bool removeAttack = false, bool preventToneJump = false)
    {
        // Play a certain note on an audioSource
        if (audioSource != null)
        {
            // Set audio source clip
            audioSource.clip = removeAttack & audioWithoutAttacks != null ? audioWithoutAttacks : fullAudio;
            // Fetch tone sample info
            int toneIndex = -1;
            if (preventToneJump == false)
            {
                // In this mode, position audio to fit the requested tone
                toneIndex = Array.FindIndex(tones, t => t.tone == tone);
            }
            else
            {
                // In this mode, prevent moving the audio: find tone info corresponding to audio position and pitch it
                int currentAudioPosition = audioSource.timeSamples;
                toneIndex = Array.FindIndex(tones, t => currentAudioPosition >= t.audioStartSamples && currentAudioPosition < t.AudioEndSamples);
            }
            // If no tone info were found, look for closest one
            if (toneIndex == -1) toneIndex = FindClosestToneIndex(tone);
            // 
            if (toneIndex != -1)
            {
                ToneInfo toneInfo = tones[toneIndex];
                float tonePitch = tone - toneInfo.tone;
                if (tonePitch != 0f && allowPitching == false)
                {
                    if (showDebug) Debug.LogWarning(name + " can't play tone " + tone + " on " + instrumentName + " without pitching audio (not allowed).");
                }
                else
                {
                    // Set audiosource pitch
                    audioSource.pitch = Mathf.Pow(PerTonePitchMultiplier, tonePitch);
                    // Set audiosource time
                    if (removeAttack)
                    {
                        if (audioSource.isPlaying == false
                            || audioSource.timeSamples < toneInfo.audioStartSamples + toneInfo.attackSamples
                            || audioSource.timeSamples >= toneInfo.AudioEndSamples)
                            audioSource.timeSamples = toneInfo.audioStartSamples + toneInfo.attackSamples;
                    }
                    else
                    {
                        if (audioSource.isPlaying == false
                            || audioSource.timeSamples < toneInfo.audioStartSamples
                            || audioSource.timeSamples >= toneInfo.AudioEndSamples)
                            audioSource.timeSamples = toneInfo.audioStartSamples;
                    }
                    // Play audio
                    if (audioSource.isPlaying == false) audioSource.Play();
                }
            }
            else
            {
                if (showDebug) Debug.LogWarning(name + " can't play tone " + tone + " on " + instrumentName);
            }
        }
        else
        {
            if (showDebug) Debug.LogWarning(name + " can't play tone " + tone + " on null AudioSource");
        }
    }

    private int FindClosestToneIndex(float tone)
    {
        int toneIndex = -1;
        float toneDiff = float.PositiveInfinity;
        // Find closest existing tone
        int toneCount = tones.Length;
        for (int t = 0; t < toneCount; t++)
        {
            if (Mathf.Abs(tone - tones[t].tone) < Mathf.Abs(toneDiff))
            {
                toneIndex = t;
                toneDiff = tone - tones[t].tone;
            }
        }
        // Return index
        return toneIndex;
    }

    private float[] PitchTone(float[] samples, int channels, float tonePitch)
    {
        float audioPitch = Mathf.Pow(PerTonePitchMultiplier, tonePitch);
        return AudioSampling.Pitch(samples, channels, audioPitch);
    }
}
