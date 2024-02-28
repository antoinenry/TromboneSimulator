using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioTrackGenerator : MonoBehaviour
{
    [Flags]
    public enum ErrorType { NoError = 0, NullInstrument = 1, NullAudio = 2, Frequency = 4, Channels = 8, Tone = 16, Duration = 32, InstrumentName = 64 }
    [Serializable]
    public struct PartVoicesIdentifier { public string partName; public bool mainVoice; public int[] alternativeVoiceIndices; }

    [Header("Input")]
    public SheetMusic trackInfo;
    public Orchestra orchestra;
    public PartVoicesIdentifier[] ignoredParts;
    [Header("Encoding")]
    public int frequency = 48000;
    public bool stereo = true;
    [Header("Audio processing")]
    public int sampleSmoothIn = 48;
    public int sampleSmoothOut = 480;
    public bool normalize = true;
    [Header("Execution")]
    public AudioClip generatedAudio;
    public float maintainFPS = 30f;
    public int minimumNotesPerFrames = 1;
    [Header("Events")]
    public UnityEvent<float> OnGenerationProgress;

    public int TotalNoteCount { get; private set; }
    public int GeneratedNoteCount { get; private set; }
    public float GenerationProgress => (float)GeneratedNoteCount / TotalNoteCount;
    public int PartCount { get; private set; }
    public int GeneratedPartCount { get; private set; }
    public string CurrentPart { get; private set; }
    public int CurrentPartLength { get; private set; }
    public int CurrentPartGeneratedNotes { get; private set; }
    public int NotesPerFrame { get; private set; }
    public bool AudioIsReady => generatedAudio != null && GeneratedNoteCount == TotalNoteCount;

    public ErrorType CheckInstrument(SamplerInstrument instrument)
    {
        ErrorType error = ErrorType.NoError;
        if (instrument != null)
        {
            if (instrument.fullAudio == null) error |= ErrorType.NullAudio;
            else
            {
                // Only supports same sample rate instruments (for now)
                if (instrument.AudioFrequency != frequency) error |= ErrorType.Frequency;
                // Only support mono and stereo instruments
                if (instrument.AudioChannels < 1 || instrument.AudioChannels > 2) error |= ErrorType.Channels;
            }
        }
        else
            error = ErrorType.NullInstrument;
        return error;
    }

    public ErrorType SampleTrack()
    {
        // Sample in a timed coroutine to avoid freezing framerate
        StopAllCoroutines();
        StartCoroutine(SampleTrackCoroutine());
        return ErrorType.NoError;
    }

    public void IgnoreVoice(string part, bool ignoreMainVoice, params int[] ignoreAlternativeVoices)
    {
        int ignoredCount = ignoredParts != null ? ignoredParts.Length : 0;
        if (ignoredCount == 0) ignoredParts = new PartVoicesIdentifier[1];
        else Array.Resize(ref ignoredParts, ignoredCount + 1);
        ignoredParts[ignoredCount] = new PartVoicesIdentifier()
        {
            partName = part,
            mainVoice = ignoreMainVoice,
            alternativeVoiceIndices = ignoreAlternativeVoices
        };
    }

    private IEnumerator SampleTrackCoroutine()
    {
        // Coroutine with timeouts to avoid freezing (eg display a loading screen)
        generatedAudio = null;
        ErrorType error = ErrorType.NoError;
        if (trackInfo != null)
        {
            // Get track length
            int channels = stereo ? 2 : 1;
            int durationSamples = Mathf.CeilToInt(trackInfo.DurationSeconds * frequency) * channels;
            float[] samples = new float[durationSamples];
            // Initialize progress
            TotalNoteCount = trackInfo.TotalNoteCount;
            PartCount = trackInfo.PartCount;
            GeneratedNoteCount = 0;
            GeneratedPartCount = 0;
            NotesPerFrame = 0;
            CurrentPart = null;
            CurrentPartLength = 0;
            CurrentPartGeneratedNotes = 0;
            // Generate track
            for (int p = 0; p < PartCount; p++)
            {
                CurrentPart = trackInfo.parts[p].instrumentName;
                CurrentPartLength = trackInfo.parts[p].NoteCount;
                CurrentPartGeneratedNotes = 0;
                bool checkPartName = InstrumentDictionary.Current.FindOfficalName(CurrentPart, out string partOfficialName);
                if (checkPartName)
                {
                    // Check if part is ignored
                    int[] ignoredVoices = null;
                    bool ignoreMainVoice = false;
                    if (ignoredParts != null)
                    {
                        PartVoicesIdentifier ignored = Array.Find(ignoredParts, x => InstrumentDictionary.SameCurrentInstruments(x.partName, partOfficialName));
                        ignoredVoices = ignored.alternativeVoiceIndices;
                        ignoreMainVoice = ignored.mainVoice;
                    }
                    // Get part notes, removing ignored voices
                    NoteInfo[] notes;
                    if (ignoreMainVoice == false && (ignoredVoices == null || ignoredVoices.Length == 0))
                    {
                        notes = trackInfo.parts[p].notes;
                    }
                    else
                    {
                        int voiceCount = trackInfo.SplitPartVoices(p, out NoteInfo[] mainVoice, out List<NoteInfo[]> otherVoices);
                        List<NoteInfo> noteList = new List<NoteInfo>(CurrentPartLength);
                        // Check if main voice is ignored
                        if (ignoreMainVoice == false) noteList.AddRange(mainVoice);
                        else
                        {
                            GeneratedNoteCount += mainVoice.Length;
                            CurrentPartGeneratedNotes += mainVoice.Length;
                        }
                        // Check alternative voices
                        if (voiceCount > 1)
                        {
                            for (int v = 0; v < voiceCount; v++)
                            {
                                if (ignoredVoices == null || Array.IndexOf(ignoredVoices, v) == -1)
                                    noteList.AddRange(otherVoices[v]);
                                else
                                {
                                    GeneratedNoteCount += otherVoices[v].Length;
                                    CurrentPartGeneratedNotes += otherVoices[v].Length;
                                }
                            }
                        }
                        notes = noteList.ToArray();
                        CurrentPartLength = notes.Length;
                    }
                    // Sample part with corresponding instrument
                    InstrumentMixInfo instrumentMix = Array.Find(orchestra.instrumentInfo, instrument => InstrumentDictionary.SameCurrentInstruments(instrument.name, partOfficialName));
                    SamplerInstrument partInstrument = instrumentMix.instrument;
                    if (partInstrument != null)
                    {
                        float[] partSamples;
                        if (Application.isPlaying && maintainFPS > 0f)
                        {
                            // Initialize samples to zero
                            float partDurationSeconds = NoteInfo.GetTotalDuration(notes);
                            int partDurationSamples = Mathf.CeilToInt(partDurationSeconds * frequency);
                            partSamples = new float[partDurationSamples * partInstrument.AudioChannels];
                            for (int n = 0; n < CurrentPartLength; n += NotesPerFrame)
                            {
                                // Adjust number of notes to sample in one frame
                                if (Time.deltaTime < 1f / maintainFPS) NotesPerFrame++;
                                else NotesPerFrame--;
                                NotesPerFrame = Mathf.Max(NotesPerFrame, minimumNotesPerFrames);
                                int sampleNotes = Mathf.Min(NotesPerFrame, CurrentPartLength - n);
                                // Sample part
                                error |= SamplePart(notes, n, sampleNotes, partInstrument, ref partSamples);
                                GeneratedNoteCount += sampleNotes;
                                CurrentPartGeneratedNotes += sampleNotes;
                                OnGenerationProgress.Invoke((float)GeneratedNoteCount / TotalNoteCount);
                                yield return null;
                            }
                        }
                        else
                        {
                            error |= SamplePart(notes, partInstrument, out partSamples);
                            GeneratedNoteCount += CurrentPartLength;
                            CurrentPartGeneratedNotes = CurrentPartLength;
                        }
                        // Convert mono/stereo
                        if (stereo && partInstrument.AudioChannels == 1) partSamples = AudioSampling.MonoToStereo(partSamples, instrumentMix.pan);
                        else if (!stereo && partInstrument.AudioChannels == 2) partSamples = AudioSampling.StereoToMono(partSamples);
                        // Apply volume
                        partSamples = Array.ConvertAll(partSamples, s => s * instrumentMix.volume);
                        // Add sampled part
                        AudioSampling.AddTo(ref samples, partSamples, 0);
                    }
                    else
                    {
                        error |= ErrorType.NullInstrument;
                        GeneratedNoteCount += CurrentPartLength;
                        CurrentPartGeneratedNotes = CurrentPartLength;
                    }
                }
                else
                {
                    error |= ErrorType.InstrumentName;
                    GeneratedNoteCount += CurrentPartLength;
                    CurrentPartGeneratedNotes = CurrentPartLength;
                }
                GeneratedPartCount++;
            }
            // Ensure audio levels are between -1f and 1f
            if (normalize) AudioSampling.Normalize(ref samples);
            // Create new clip
            generatedAudio = AudioClip.Create(trackInfo.name, durationSamples / channels, channels, frequency, false);
            // Transfer samples to audio clip
            generatedAudio.SetData(samples, 0);
        }
    }

    private ErrorType SamplePart(NoteInfo[] notes, SamplerInstrument instrument, out float[] partSamples)
    {
        ErrorType error = ErrorType.NoError;
        if (notes != null)
        {
            // Initialize samples to zero
            float partDurationSeconds = NoteInfo.GetTotalDuration(notes);
            int partDurationSamples = Mathf.CeilToInt(partDurationSeconds * frequency);
            partSamples = new float[partDurationSamples * instrument.AudioChannels];
            // Place note samples
            error = SamplePart(notes, 0, notes.Length, instrument, ref partSamples);
        }
        else
            partSamples = null;
        return error;
    }

    private ErrorType SamplePart(NoteInfo[] notes, int startAtNote, int noteCount, SamplerInstrument instrument, ref float[] partSamples)
    {
        ErrorType error = ErrorType.NoError;
        if (notes != null && instrument != null)
        {
            // Apply performance style
            NoteInfo[] performedNotes;
            if (instrument.style != null) performedNotes = instrument.style.ProcessNotes(notes);
            else performedNotes = notes;
            // Place note samples
            for (int n = Mathf.Max(0, startAtNote), nend = Mathf.Min(notes.Length, startAtNote + noteCount); n < nend; n++)
            {
                NoteInfo note = performedNotes[n];
                // Check previous note for legatos (play/remove attack)
                bool removeAttack = n > 0 && note.startTime == performedNotes[n - 1].EndTime;
                // Sample note
                error |= SampleNote(note, instrument, removeAttack, out float[] noteSamples, out int sampleOffset);
                int notePositionSamples = Mathf.FloorToInt(note.startTime * frequency + sampleOffset);
                AudioSampling.AddTo(ref partSamples, noteSamples, notePositionSamples * instrument.AudioChannels);
            }
        }
        return error;

    }

    private ErrorType SampleNote(NoteInfo note, SamplerInstrument instrument, bool removeAttack, out float[] samples, out int sampleOffset)
    {
        samples = new float[0];
        sampleOffset = 0;
        ErrorType error = CheckInstrument(instrument);
        if (error == ErrorType.NoError)
        {
            if (instrument.TrySampleNote(note, removeAttack, out samples, out int attackSamples))
            {
                // Offset note to compensate attack
                sampleOffset = -attackSamples;
                // Smooth samples to avoid audio popping
                AudioSampling.FadeIn(ref samples, instrument.AudioChannels, sampleSmoothIn);
                AudioSampling.FadeOut(ref samples, instrument.AudioChannels, sampleSmoothOut);
                return ErrorType.NoError;
            }
            else
            {
                error = ErrorType.Tone;
            }
        }
        return error;
    }
}
