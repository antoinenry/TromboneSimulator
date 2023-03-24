using UnityEngine;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.IO;

public class MIDIExtractor
{
    [Flags]
    public enum ErrorType { Succes = 0, NoFile = 1, NoTempoTrack = 2, SeveralTempoTracks = 4, OpenNote = 8 }

    public ErrorType result;

    private short ticksPerQuarterNote;

    public void TryExtractFile(string filePath, SheetMusic extracted)
    {
        result = ErrorType.Succes;
        if (File.Exists(filePath))
        {
            MidiFile midi = MidiFile.Read(filePath);
            // Get time scale to read MIDI events (MIDI time is measured in ticks)
            ticksPerQuarterNote = midi.TimeDivision is TicksPerQuarterNoteTimeDivision ?
                (midi.TimeDivision as TicksPerQuarterNoteTimeDivision).TicksPerQuarterNote :
                TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote;
            // Get tempo changes
            TrackChunk[] tempoChunks = FindChunksWithEventType(midi, MidiEventType.SetTempo);
            if (tempoChunks.Length == 1)
                extracted.tempo = GetTempoChanges(tempoChunks[0].Events);
            else
            {
                Debug.LogWarning("Incorrect tempo chunks count (" + tempoChunks.Length + ")");
                result |= tempoChunks.Length == 0 ? ErrorType.NoTempoTrack : ErrorType.SeveralTempoTracks;
            }
            // Get meter changes
            TrackChunk[] timeSignatureChunks = FindChunksWithEventType(midi, MidiEventType.TimeSignature);
            if (timeSignatureChunks.Length == 1)
                extracted.measure = GetMeasureChanges(timeSignatureChunks[0].Events);
            else
            {
                Debug.LogWarning("Incorrect signature chunks count (" + timeSignatureChunks.Length + ")");
                result |= timeSignatureChunks.Length == 0 ? ErrorType.NoTempoTrack : ErrorType.SeveralTempoTracks;
            }
            // Get instrument parts
            TrackChunk[] noteChunks = FindChunksWithEventType(midi, MidiEventType.NoteOn, MidiEventType.NoteOff);
            extracted.PartCount = noteChunks.Length;
            // Get notes
            int c = 0;
            foreach (TrackChunk chunk in noteChunks)
            {
                extracted.parts[c].instrumentName = GetPartName(chunk.Events);
                extracted.parts[c].notes = GetNotes(chunk.Events, extracted.tempo);
                c++;
            }
        }
        else
        {
            Debug.LogWarning("MIDI Load failed at" + filePath);
            result |= ErrorType.NoFile;
        }
    }

    private bool ChunkContainsEventType(TrackChunk chunk, params MidiEventType[] eventTypes)
    {
        if (chunk != null && eventTypes != null)
        {
            foreach (MidiEvent e in chunk.Events)
            {
                foreach (MidiEventType type in eventTypes)
                    if (e.EventType == type) return true;
            }
        }
        return false;
    }

    private TrackChunk[] FindChunksWithEventType(MidiFile midi, params MidiEventType[] eventTypes)
    {
        List<TrackChunk> chunks = new List<TrackChunk>();
        if (midi != null)
        {
            IEnumerable<TrackChunk> midiChunks = midi.GetTrackChunks();
            if (midiChunks != null)
                foreach (TrackChunk chunk in midiChunks)
                    if (ChunkContainsEventType(chunk, eventTypes)) chunks.Add(chunk);
        }
        return chunks.ToArray();
    }

    private string GetPartName(EventsCollection events)
    {
        string name = "";
        foreach (MidiEvent e in events)
        {
            string text = null;
            switch(e.EventType)
            {
                case MidiEventType.SequenceTrackName: text = (e as SequenceTrackNameEvent).Text;
                    break;
                case MidiEventType.InstrumentName: text = (e as InstrumentNameEvent).Text;
                    break;
            }
            if (text != null)
            {
                if (name == "") name = text;
                else name += "/" + text;
            }
        }
        return name;
    }

    private TempoInfo[] GetTempoChanges(EventsCollection events)
    {
        List<TempoInfo> readTempo = new List<TempoInfo>();
        // Initialize tempo infos
        long microSecondsPerQuarterNote = 0L;
        // Read events
        long timeMicroSeconds = 0L;
        foreach (MidiEvent e in events)
        {
            // Time increment
            timeMicroSeconds += e.DeltaTime * microSecondsPerQuarterNote / ticksPerQuarterNote;
            // Tempo change
            if (e.EventType == MidiEventType.SetTempo)
            {
                if (microSecondsPerQuarterNote != (e as SetTempoEvent).MicrosecondsPerQuarterNote)
                {
                    microSecondsPerQuarterNote = (e as SetTempoEvent).MicrosecondsPerQuarterNote;
                    TempoInfo tempo = new TempoInfo(timeMicroSeconds / 1000000f, microSecondsPerQuarterNote / 1000000f);
                    if (readTempo.Contains(tempo) == false) readTempo.Add(tempo);
                }
            }
        }
        return readTempo.ToArray();
    }

    private MeasureInfo[] GetMeasureChanges(EventsCollection events)
    {
        List<MeasureInfo> readMeasure = new List<MeasureInfo>();
        // Initialize measure infos
        float bars = 0;
        float quarterNotesPerBar = 4f;
        // Read events
        foreach (MidiEvent e in events)
        {
            // Bar increment
            bars += e.DeltaTime / (ticksPerQuarterNote * quarterNotesPerBar);
            if (e.EventType == MidiEventType.TimeSignature)
            {
                // 4/4 is the default time signature, add it at the start of the track if there's no event at time = 0
                if (readMeasure.Count == 0 && bars != 0f) readMeasure.Add(new MeasureInfo(0));
                // Time signature event
                MeasureInfo measure = new MeasureInfo(Mathf.RoundToInt(bars), (e as TimeSignatureEvent).Numerator, (e as TimeSignatureEvent).Denominator);
                if (readMeasure.Contains(measure) == false) readMeasure.Add(measure);
                quarterNotesPerBar = measure.QuarterNotesPerBar;
                // Signal if a significant rounding of the bar time was made
                if (Mathf.Approximately(bars, Mathf.Round(bars)) == false)
                {
                    Debug.LogWarning("Signature change (" +
                        (e as TimeSignatureEvent).Numerator + "/" + (e as TimeSignatureEvent).Denominator +
                        ") at incomplete measure: " + bars + ". Bar time was rounded to " + measure.bar +".");
                }
            }
        }
        // If not measure changes, set default measure
        if (readMeasure.Count == 0) readMeasure.Add(new MeasureInfo(0));
        return readMeasure.ToArray();       
    }

    private NoteInfo[] GetNotes(EventsCollection events, TempoInfo[] tempoChanges)
    {
        List<NoteInfo> readNotes = new List<NoteInfo>();
        // Read events
        float time = 0f;
        foreach (MidiEvent e in events)
        {
            // Time increment
            time = MoveTimeByTicks(time, e.DeltaTime, tempoChanges);
            // Note start
            if (e.EventType == MidiEventType.NoteOn)
            {
                NoteInfo openNote = new NoteInfo()
                {
                    tone = (e as NoteOnEvent).NoteNumber,
                    velocity = (e as NoteOnEvent).Velocity / 128f,
                    startTime = time,
                    duration = 0f
                };
                if (readNotes.Contains(openNote) == false) readNotes.Add(openNote);
            }
            // Note end
            else if (e.EventType == MidiEventType.NoteOff)
            {
                // Find open note with same tone (open = zero duration)
                int noteIndex = readNotes.FindIndex(n => n.duration == 0f && n.tone == (e as NoteOffEvent).NoteNumber);
                if (noteIndex >= 0)
                {
                    NoteInfo closedNote = readNotes[noteIndex];
                    closedNote.EndTime = time;
                    readNotes[noteIndex] = closedNote;
                }
                else
                {
                    result |= ErrorType.OpenNote;
                    Debug.LogWarning("No NoteOn event matches NoteOff event " + e.ToString());
                }
            }
        }
        return readNotes.ToArray();
    }

    private float MoveTimeByTicks(float time, long deltaTicks, TempoInfo[] tempoChanges)
    {
        // Move time in seconds by delta in ticks, applying tempo changes
        long ticks = 0;
        float movedSeconds = time;
        if (tempoChanges != null)
        {
            // Move across tempo changes
            for (int t = 0, tempoCount = tempoChanges.Length; t < tempoCount; t++)
            {
                // Get tempo infos
                TempoInfo tempo = tempoChanges[t];
                float ticksPerSeconds = ticksPerQuarterNote / tempo.secondsPerQuarterNote;
                float tempoDurationSeconds = t < tempoCount - 1 ? tempoChanges[t + 1].time - tempo.time : float.PositiveInfinity;
                // End time is in this tempo
                if (ticks + tempoDurationSeconds * ticksPerSeconds >= deltaTicks)
                {
                    movedSeconds += (deltaTicks - ticks) / ticksPerSeconds;
                    break;
                }
                // End time is further, move to next tempo
                else
                {
                    movedSeconds += tempoDurationSeconds / ticksPerSeconds;
                    ticks += deltaTicks;
                }
            }

        }
        return movedSeconds;
    }
}
