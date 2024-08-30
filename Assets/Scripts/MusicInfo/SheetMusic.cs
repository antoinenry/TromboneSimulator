using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSheetMusic", menuName = "Trombone Hero/Sheet Music")]
public class SheetMusic : ScriptableObject
{
    [Header("Info")]
    public string title;
    public string subtitle;
    public string composer;
    [Header("Music")]
    [SerializeField] private TempoInfo[] tempo;
    [SerializeField] private MeasureInfo[] measure;
    [SerializeField] private SheetMusicPart[] parts;
    [SerializeField] private bool completeLastBar = true;

    public bool CreatedAtRuntime { get; private set; }

    private void Awake()
    {
        CreatedAtRuntime = Application.isPlaying;
    }

    #region Write music
    public void SetTempo(TempoInfo[] t)
    {
        int length = t != null ? t.Length : 0;
        tempo = new TempoInfo[length];
        Array.Copy(t, tempo, length);
    }

    public void SetMeasure(MeasureInfo[] m)
    {
        int length = m != null ? m.Length : 0;
        measure = new MeasureInfo[length];
        Array.Copy(m, measure, length);
    }

    public void MultiplyTempoBy(float tempoModifier)
    {
        if (tempoModifier == 0f || tempoModifier == 1f) return;
        float timeScale = 1f / tempoModifier;
        if (tempo != null) tempo = Array.ConvertAll(tempo, t => t.ScaleTime(timeScale));
        if (parts != null) parts = Array.ConvertAll(parts, p => p.ScaleTime(timeScale));
    }

    public void SetPart(int index, string instrumentName, NoteInfo[] notes)
    {
        SheetMusicPart part = parts[index];
        int noteCount = notes != null ? notes.Length : 0;
        part.name = instrumentName;
        part.notes = new NoteInfo[noteCount];
        Array.Copy(notes, part.notes, noteCount);
        parts[index] = part;
    }

    public void TransposePart(int partIndex, float byTones)
    {
        if (partIndex >= 0 && partIndex < PartCount)
            parts[partIndex] = parts[partIndex].Transpose(byTones);
    }

    public void TransposePart(string instrumentName, float tones)
    {
        int partIndex = parts != null ? Array.FindIndex(parts, p => p.name == instrumentName) : -1;
        if (partIndex != -1) TransposePart(partIndex, tones);
    }

    public void TransposeBy(float tones)
    {
        // Transpose every part (except drums)
        for (int p = 0; p < PartCount; p++)
        {
            if (InstrumentDictionary.IsCurrentDrums(parts[p].name) == false)
                TransposePart(p, tones);
        }
    }

    public NoteInfo GetNote(int partIndex, int noteIndex)
    {
        if (partIndex < 0 || partIndex >= PartCount) return NoteInfo.None;
        return parts[partIndex].GetNote(noteIndex);
    }

    public NoteInfo GetNote(string instrumentName, int noteIndex)
        => GetNote(GetPartIndex(instrumentName), noteIndex);

    public void SetNote(int partIndex, int noteIndex, NoteInfo noteInfo)
    {
        if (partIndex < 0 || partIndex >= PartCount) return;
        parts[partIndex].SetNote(noteIndex, noteInfo);
    }

    public void SetNote(string instrumentName, int noteIndex, NoteInfo noteInfo)
        => SetNote(GetPartIndex(instrumentName), noteIndex, noteInfo);
    #endregion

    #region Read Music
    public int GetTotalNoteCount()
    {
        int count = 0;
        if (parts != null)
            foreach (SheetMusicPart p in parts) count += p.NoteCount;
        return count;
    }

    public float GetDuration()
    {
        float duration = 0f;
        if (parts != null)
            foreach (SheetMusicPart p in parts)
                duration = Mathf.Max(duration, NoteInfo.GetTotalDuration(p.notes));
        if (completeLastBar)
        {
            MetronomeTrack rythmTrack = new MetronomeTrack(tempo, measure);
            if (rythmTrack.IsReady)
            {
                int lastBarIndex = rythmTrack.GetBarIndex(duration);
                float lastBarStartTime = rythmTrack.GetBarStartTime(lastBarIndex);
                if (lastBarStartTime != duration) duration = lastBarStartTime + rythmTrack.GetBarDuration(lastBarIndex); ;
            }
        }
        return Mathf.Abs(duration);
    }

    public TempoInfo[] GetTempo()
        => tempo != null ? (TempoInfo[])tempo.Clone() : new TempoInfo[0];

    public MeasureInfo[] GetMeasure()
        => (MeasureInfo[])measure?.Clone();

    public NoteInfo[] GetPartNotes(int partIndex)
    {
        if (parts != null && partIndex >= 0 && partIndex < parts.Length && parts[partIndex].notes != null)
            return (NoteInfo[])parts[partIndex].notes.Clone();
        else
            return null;
    }

    public NoteInfo[] GetPartNotes(string partName)
        => GetPartNotes(GetPartIndex(partName));

    public NoteInfo[] GetPartNotes(int partIndex, bool includeMainVoice, params int[] voiceIndices)
    {
        if (includeMainVoice == false && (voiceIndices == null || voiceIndices.Length == 0)) return new NoteInfo[0];
        NoteInfo[] notes = GetPartNotes(partIndex);
        int voiceCount = SplitPartVoices(partIndex, out int[] mainNoteIndices, out List<int[]> alternativeNoteIndices);
        if (voiceCount < 1) return new NoteInfo[0];
        // Get notes by assembling selected voices
        List<int> selectedNoteIndices = includeMainVoice ? new List<int>(mainNoteIndices) : new List<int>();
        if (alternativeNoteIndices != null)
            foreach (int selectedVoiceIndex in voiceIndices)
            {
                if (selectedVoiceIndex < 0 || selectedVoiceIndex >= voiceCount) continue;
                selectedNoteIndices.AddRange(alternativeNoteIndices[selectedVoiceIndex]);
            }
        return selectedNoteIndices.ConvertAll(i => notes[i]).ToArray();
    }

    public NoteInfo[] GetPartNotes(string partName, bool includeMainVoice, params int[] voiceIndices)
        => GetPartNotes(GetPartIndex(partName), includeMainVoice, voiceIndices);

    public NoteInfo[] GetPartNotes(SheetMusicVoiceIdentifier part)
        => GetPartNotes(part.partName, part.mainVoice, part.alternativeVoiceIndices);

    //public NotePlay[] GetVoiceNotes(SamplerInstrument instrument, int voiceIndex)
    //{
    //    int partIndex = GetPartIndex(instrument.name);
    //    NoteInfo[] notes = GetPartNotes(partIndex);
    //    int voices = SplitPartVoices(partIndex, out int[] mainNoteIndices, out List<int[]> alternativeNoteIndices);
    //    if (voices < 1) return new NotePlay[0];
    //    else
    //    {
    //        // Get notes
    //        int[] getNoteIndices = voices == 1 ? mainNoteIndices : NoteInfo.Assemble(notes, mainNoteIndices, alternativeNoteIndices[voiceIndex]);
    //        int noteCount = getNoteIndices != null ? getNoteIndices.Length : 0;
    //        if (noteCount == 0) return new NotePlay[0];
    //        // Process style
    //        NoteInfo[] getNoteInfos = Array.ConvertAll(getNoteIndices, i => notes[i]);
    //        if (instrument.style != null) instrument.style.ProcessNotes(getNoteInfos);
    //        // Return notes
    //        NotePlay[] getNotes = new NotePlay[noteCount];
    //        for (int i = 0; i < noteCount; i++) getNotes[i] = new NotePlay(getNoteInfos[i], getNoteIndices[i]);
    //        return getNotes;
    //    }
    //}

    public bool MusicEquals(SheetMusic other, float tempoRatio = 1f, float transposition = 0f)
    {
        if (other == null) return false;
        float timeScale = tempoRatio != 0f ? 1f / tempoRatio : 0f;
        if (!Enumerable.SequenceEqual(measure, other.measure)) return false;
        TempoInfo[] modifiedTempo = TempoInfo.ScaleTime(other.tempo, timeScale);
        if (!Enumerable.SequenceEqual(tempo, modifiedTempo)) return false;
        SheetMusicPart[] modifiedParts = Array.ConvertAll(other.parts, p => p.Transpose(transposition).ScaleTime(timeScale));
        if (!Enumerable.SequenceEqual(parts, modifiedParts)) return false;
        return true;
    }
    #endregion

    #region Parts
    public int PartCount
    {
        get => parts != null ? parts.Length : 0;
        set => Array.Resize(ref parts, Mathf.Max(value, 0));
    }

    public string[] PartNames
    {
        get => parts != null ? Array.ConvertAll(parts, p => p.name) : null;
    }

    public int GetPartIndex(string partName)
    {
        if (parts == null || !InstrumentDictionary.FindCurrentOfficalName(partName, out string searchedOfficalName)) return -1;
        return Array.FindIndex(parts, p => InstrumentDictionary.FindCurrentOfficalName(p.name, out string partOfficialName) && partOfficialName == searchedOfficalName);
    }

    public int[] FindPartIndices(string partName)
    {
        if (parts == null || !InstrumentDictionary.FindCurrentOfficalName(partName, out string officialName1)) return new int[0];
        List<int> partIndices = new List<int>();
        for (int i = 0; i < PartCount; i++)
        {
            if (parts[i].name == partName 
                || (InstrumentDictionary.FindCurrentOfficalName(parts[i].name, out string officialName2) && officialName1 == officialName2))
                partIndices.Add(i);
        }
        return partIndices.ToArray();
    }

    public string FindPartsWithSameName(out int[] partIndices)
    {
        foreach(string partName in PartNames)
        {
            partIndices = FindPartIndices(partName);
            if (partIndices.Length > 1) return partName;
        }
        partIndices = null;
        return null;
    }

    public string GetPartInstrument(int partIndex)
    {
        if (parts != null && partIndex >= 0 && partIndex < parts.Length) return parts[partIndex].name;
        else return null;
    }

    public int GetPartLength(int partIndex)
    {
        if (parts != null && partIndex >= 0 && partIndex < parts.Length) return parts[partIndex].NoteCount;
        else return 0;
    }

    public int GetPartLength(string partName) => GetPartLength(GetPartIndex(partName));

    public float GetTotalNoteTime(int partIndex)
    {
        NoteInfo[] notes = GetPartNotes(partIndex);
        if (notes == null) return 0f;
        float totalNoteTime = 0f;
        foreach (NoteInfo n in notes) totalNoteTime += n.duration;
        return totalNoteTime;
    }
    #endregion

    #region Voices
    public int GetVoiceCount(int partIndex) => SplitPartVoices(partIndex, out int[] main, out List<int[]> alternatives);

    public int SplitPartVoices(int partIndex, out int[] mainNotes, out List<int[]> alternativeNotes)
    {
        mainNotes = null;
        alternativeNotes = null;
        // Get notes
        NoteInfo[] notes = GetPartNotes(partIndex);
        if (notes == null) return 0;
        // Order notes by time (from first to last) and tone (from high to low)
        notes = NoteInfo.OrderNotes(notes, true, false, true, true);
        int noteCount = notes.Length;
        // Scope notes and split into voices when notes are ovelapping
        bool[] processed = new bool[noteCount];
        int processedCount = 0;
        List<int> newVoice = new List<int>(noteCount);
        // Fill main voice (index 0) with notes that are isolated (no overlapping notes)
        for (int n = 0; n < noteCount; n++)
        {
            // Find overlapping notes
            int overlap = 0;
            for (int o = n + 1; o < noteCount; o++)
            {
                if (notes[o].startTime < notes[n].EndTime) overlap++;
                else break;
            }
            // Add note only if no overlapping note was found
            if (overlap == 0)
            {
                newVoice.Add(n);
                processed[n] = true;
                processedCount++;
            }
            // Else skip overlapping notes
            else
                n += overlap;
        }
        // Add main voice
        mainNotes = newVoice.ToArray();
        // Stop here if main voice contains all notes
        if (processedCount == noteCount) return 1;
        // Distribute other notes into secondary voices
        alternativeNotes = new List<int[]>();
        while (processedCount < noteCount)
        {
            newVoice = new List<int>(noteCount - processedCount);
            // Fill a new voice with non-overlapping notes
            for (int n = 0; n < noteCount; n++)
            {
                if (processed[n] == true) continue;
                newVoice.Add(n);
                processed[n] = true;
                processedCount++;
                // Skip overlapping notes
                int overlap = 0;
                for (int o = n + 1; o < noteCount; o++)
                {
                    if (notes[o].startTime < notes[n].EndTime) overlap++;
                    else break;
                }
                n += overlap;
            }
            // Add new voice
            alternativeNotes.Add(newVoice.ToArray());
        }
        // Return voice count
        if (alternativeNotes.Count == 0) return mainNotes.Length > 0 ? 1 : 0;
        else return alternativeNotes.Count;
    }

    public int SplitPartVoices(int partIndex, out NoteInfo[] mainNotes, out List<NoteInfo[]> alternativeNotes)
    {
        mainNotes = null;
        alternativeNotes = null;
        int voiceCount = SplitPartVoices(partIndex, out int[] mainNoteIndices, out List<int[]> alternativeNoteIndices);
        if (voiceCount == 0) return 0;
        NoteInfo[] notes = GetPartNotes(partIndex);
        mainNotes = Array.ConvertAll(mainNoteIndices, n => notes[n]);
        if (voiceCount > 1)
        {
            alternativeNotes = new List<NoteInfo[]>(voiceCount);
            foreach (int[] alternativeVoice in alternativeNoteIndices)
                alternativeNotes.Add(Array.ConvertAll(alternativeVoice, n => notes[n]));
        }
        return voiceCount;
    }
    public void RemovePart(int partIndex)
    {
        if (parts == null || partIndex < 0 || partIndex >= parts.Length) return;
        for (int i = partIndex; i < PartCount - 1; i++) parts[i] = parts[i + 1];
        Array.Resize(ref parts, PartCount - 1);
    }

    public void MergeParts(params int[] partIndices)
    {
        int mergedPartsCount = partIndices != null ? partIndices.Length : 0;
        if (mergedPartsCount <= 1) return;
        int partIndex1 = partIndices[0];
        if (parts == null || partIndex1 < 0 || partIndex1 >= parts.Length) return;
        for (int i = 1; i < mergedPartsCount; i++)
        {
            int partIndex2 = partIndices[i];
            if (partIndex2 < 0 || partIndex2 >= parts.Length) continue;
            parts[partIndex1] = SheetMusicPart.Merge(parts[partIndex1], parts[partIndex2]);
        }
        for (int i = 1; i < mergedPartsCount; i++)
            RemovePart(partIndices[i]);
    }
    #endregion


    // -----------------------------
    // Move to InstrumentDictionnary
    public int FindOutOfRangeTones(out string[] instrumentNames, out float[] outOfRangeTones)
    {
        int totalOutOfRangeCount = 0;
        List<float> toneList = new List<float>();
        List<string> instrumentList = new List<string>();
        foreach (SheetMusicPart p in parts)
        {
            if (InstrumentDictionary.IsCurrentDrums(p.name)) continue;
            int outOfRangeCount = FindOutOfRangeTones(p, ref toneList);
            totalOutOfRangeCount += outOfRangeCount;
            for (int i = 0; i < outOfRangeCount; i++) instrumentList.Add(p.name);
        }
        outOfRangeTones = toneList.ToArray();
        instrumentNames = instrumentList.ToArray();
        return totalOutOfRangeCount;
    }

    public int FindOutOfRangeTones(SheetMusicPart part, ref List<float> outOfRangeTones)
    {
        int outOfRangeCount = 0;
        foreach (NoteInfo note in part.notes)
        {
            float tone = note.tone;
            if (InstrumentDictionary.CheckCurrentToneRange(tone, part.name) == false)
            {
                outOfRangeCount++;
                if (outOfRangeTones.Contains(tone) == false) outOfRangeTones.Add(tone);
            }
        }
        return outOfRangeCount;
    }

    public int FindUnknownDrumHits(out float[] undefinedTones)
    {
        int undefinedCount = 0;
        List<float> toneList = new List<float>();
        SheetMusicPart[] drumParts = Array.FindAll(parts, p => InstrumentDictionary.IsCurrentDrums(p.name));
        foreach (SheetMusicPart part in drumParts)
        {
            undefinedCount += FindUnknownDrumHits(part, ref toneList);
        }
        undefinedTones = toneList.ToArray();
        return undefinedCount;
    }

    public int FindUnknownDrumHits(SheetMusicPart part, ref List<float> undefinedTones)
    {
        int undefinedCount = 0;
        foreach (NoteInfo note in part.notes)
        {
            float tone = note.tone;
            if (InstrumentDictionary.FindCurrentDrumHitName(tone, out string hit) == false)
            {
                undefinedCount++;
                if (undefinedTones.Contains(tone) == false) undefinedTones.Add(tone);
            }
        }
        return undefinedCount;
    }
}
