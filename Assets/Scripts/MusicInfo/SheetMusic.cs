using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSheetMusic", menuName = "Trombone Hero/Music/Sheet Music")]
public class SheetMusic : ScriptableObject
{
    [Serializable]
    public struct Part
    {
        public string instrumentName;
        public NoteInfo[] notes;

        public int NoteCount => notes != null ? notes.Length : 0;
    }

    [SerializeField] private TempoInfo[] tempo;
    [SerializeField] private MeasureInfo[] measure;
    [SerializeField] private Part[] parts;
    [SerializeField] private bool completeLastBar = true;

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

    public void SetPart(int index, string instrumentName, NoteInfo[] notes)
    {
        Part part = parts[index];
        int noteCount = notes != null ? notes.Length : 0;
        part.instrumentName = instrumentName;
        part.notes = new NoteInfo[noteCount];
        Array.Copy(notes, part.notes, noteCount);
        parts[index] = part;
    }

    public void TransposePart(int partIndex, float tones)
    {
        if (partIndex >= 0 && partIndex < PartCount)
        {
            int noteCount = parts[partIndex].notes != null ? parts[partIndex].notes.Length : 0;
            for (int n = 0; n < noteCount; n++)
            {
                NoteInfo note = parts[partIndex].notes[n];
                note.tone += tones;
                parts[partIndex].notes[n] = note;
            }
        }
    }

    public void TransposePart(string instrumentName, float tones)
    {
        if (parts != null)
        {
            int partIndex = Array.FindIndex(parts, p => p.instrumentName == instrumentName);
            if (partIndex != -1) TransposePart(partIndex, tones);
        }
    }

    public void Transpose(float tones)
    {
        // Transpose every part (except drums)
        for (int p = 0; p < PartCount; p++)
        {
            if (InstrumentDictionary.IsCurrentDrums(parts[p].instrumentName) == false)
                TransposePart(p, tones);
        }
    }
    #endregion

    #region Read Music
    public int GetTotalNoteCount()
    {
        int count = 0;
        if (parts != null)
            foreach (Part p in parts) count += p.NoteCount;
        return count;
    }

    public float GetDuration(float timeStretch = 1f)
    {
        float duration = 0f;
        if (parts != null)
            foreach (Part p in parts)
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
        return Mathf.Abs(duration * timeStretch);
    }

    public TempoInfo[] GetTempo(float timeStretch = 1f)
        => tempo != null ? Array.ConvertAll(tempo, t => t * timeStretch) : new TempoInfo[0];

    public MeasureInfo[] GetMeasure()
        => (MeasureInfo[])measure?.Clone();

    public NoteInfo[] GetPartNotes(int partIndex, float timeStretch = 1f)
    {
        if (parts != null && partIndex >= 0 && partIndex < parts.Length && parts[partIndex].notes != null)
            return Array.ConvertAll(parts[partIndex].notes, n => n.ScaleTime(timeStretch));
        else
            return null;
    }

    public NoteInfo[] GetPartNotes(string partName, float timeStretch = 1f)
        => GetPartNotes(GetPartIndex(partName), timeStretch);    

    public NotePlay[] GetVoiceNotes(SamplerInstrument instrument, int voiceIndex, float timeStretch = 1f)
    {
        if (instrument == null) return null;
        int partIndex = GetPartIndex(instrument.instrumentName);
        NoteInfo[] notes = GetPartNotes(partIndex, timeStretch);
        int voices = SplitPartVoices(partIndex, out int[] mainNoteIndices, out List<int[]> alternativeNoteIndices);
        if (voices < 1) return new NotePlay[0];
        else
        {
            // Get notes
            int[] getNoteIndices = voices == 1 ? mainNoteIndices : NoteInfo.Assemble(notes, mainNoteIndices, alternativeNoteIndices[voiceIndex]);
            int noteCount = getNoteIndices != null ? getNoteIndices.Length : 0;
            if (noteCount == 0) return new NotePlay[0];
            // Process style
            NoteInfo[] getNoteInfos = Array.ConvertAll(getNoteIndices, i => notes[i]);
            if (instrument.style != null) instrument.style.ProcessNotes(getNoteInfos);
            // Return notes
            NotePlay[] getNotes = new NotePlay[noteCount];
            for (int i = 0; i < noteCount; i++) getNotes[i] = new NotePlay(getNoteInfos[i], getNoteIndices[i]);
            return getNotes;
        }
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
        get => parts != null ? Array.ConvertAll(parts, p => p.instrumentName) : null;
    }

    public int GetPartIndex(string partName)
    {
        if (parts != null && InstrumentDictionary.FindCurrentOfficalName(partName, out string searchedOfficalName))
        {
            int partIndex = Array.FindIndex(parts, p => InstrumentDictionary.FindCurrentOfficalName(p.instrumentName, out string partOfficialName) && partOfficialName == searchedOfficalName);
            if (partIndex != -1) return partIndex;
        }
        return -1;
    }

    public string GetPartInstrument(int partIndex)
    {
        if (parts != null && partIndex >= 0 && partIndex < parts.Length) return parts[partIndex].instrumentName;
        else return null;
    }

    public int GetPartLength(int partIndex)
    {
        if (parts != null && partIndex >= 0 && partIndex < parts.Length) return parts[partIndex].NoteCount;
        else return 0;
    }

    public int GetPartLength(string partName) => GetPartLength(GetPartIndex(partName));
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

    public int SplitPartVoices(int partIndex, out NoteInfo[] mainNotes, out List<NoteInfo[]> alternativeNotes, float timeStretch = 1f)
    {
        mainNotes = null;
        alternativeNotes = null;
        int voiceCount = SplitPartVoices(partIndex, out int[] mainNoteIndices, out List<int[]> alternativeNoteIndices);
        if (voiceCount == 0) return 0;
        NoteInfo[] notes = GetPartNotes(partIndex, timeStretch);
        mainNotes = Array.ConvertAll(mainNoteIndices, n => notes[n]);
        if (voiceCount > 1)
        {
            alternativeNotes = new List<NoteInfo[]>(voiceCount);
            foreach (int[] alternativeVoice in alternativeNoteIndices)
                alternativeNotes.Add(Array.ConvertAll(alternativeVoice, n => notes[n]));
        }
        return voiceCount;
    }
    #endregion


    // -----------------------------
    // Move to InstrumentDictionnary
    public int FindOutOfRangeTones(out string[] instrumentNames, out float[] outOfRangeTones)
    {
        int totalOutOfRangeCount = 0;
        List<float> toneList = new List<float>();
        List<string> instrumentList = new List<string>();
        foreach (Part p in parts)
        {
            if (InstrumentDictionary.IsCurrentDrums(p.instrumentName)) continue;
            int outOfRangeCount = FindOutOfRangeTones(p, ref toneList);
            totalOutOfRangeCount += outOfRangeCount;
            for (int i = 0; i < outOfRangeCount; i++) instrumentList.Add(p.instrumentName);
        }
        outOfRangeTones = toneList.ToArray();
        instrumentNames = instrumentList.ToArray();
        return totalOutOfRangeCount;
    }

    public int FindOutOfRangeTones(Part part, ref List<float> outOfRangeTones)
    {
        int outOfRangeCount = 0;
        foreach (NoteInfo note in part.notes)
        {
            float tone = note.tone;
            if (InstrumentDictionary.CheckCurrentToneRange(tone, part.instrumentName) == false)
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
        Part[] drumParts = Array.FindAll(parts, p => InstrumentDictionary.IsCurrentDrums(p.instrumentName));
        foreach (Part part in drumParts)
        {
            undefinedCount += FindUnknownDrumHits(part, ref toneList);
        }
        undefinedTones = toneList.ToArray();
        return undefinedCount;
    }

    public int FindUnknownDrumHits(Part part, ref List<float> undefinedTones)
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
