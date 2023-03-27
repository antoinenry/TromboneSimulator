using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface INote 
{
    public float Tone { get; set; }
    public float Velocity { get; set; }
    public float StartTime { get; set; }
    public float Duration { get; set; }
}

[Serializable]
public struct NoteInfo
{
    public class NoteComparer : IComparer<NoteInfo>
    {
        public bool compareStartTime;
        public bool compareTone;
        public bool invertTime;
        public bool invertTone;

        public int Compare(NoteInfo x, NoteInfo y)
        {
            if (compareStartTime)
            {
                if (x.startTime > y.startTime) return invertTime ? -1 : 1;
                if (x.startTime < y.startTime) return invertTime ? 1 : -1;
            }
            if (compareTone)
            {
                if (x.tone > y.tone) return invertTone ? -1 : 1;
                if (x.tone < y.tone) return invertTone ? 1 : -1;
            }
            return 0;
        }
    }

    [Tone] public float tone;
    [Range(0f, 1f)] public float velocity;
    public float startTime;
    public float duration;

    public float EndTime
    {
        get => startTime + duration;
        set => duration = Mathf.Max(0f, value - startTime);
    }

    static public NoteInfo None => new NoteInfo()
    {
        tone = 0f,
        velocity = 0f,
        startTime = 0f,
        duration = 0f
    };

    static public NoteInfo GetInfo(INote note)
    {
        return note != null ? new NoteInfo()
        {
            tone = note.Tone,
            velocity = note.Velocity,
            startTime = note.StartTime,
            duration = note.Duration
        }
        : None;
    }

    static public void SetInfo(INote note, NoteInfo info)
    {
        if (note != null)
        {
            note.Tone = info.tone;
            note.Velocity = info.velocity;
            note.StartTime = info.startTime;
            note.Duration = info.duration;
        }
    }

    public void Copy(NoteInfo copyFrom)
    {
        tone = copyFrom.tone;
        velocity = copyFrom.velocity;
        startTime = copyFrom.startTime;
        duration = copyFrom.duration;
    }

    static public float GetTotalDuration(NoteInfo[] notes)
    {
        float duration = 0f;
        if (notes != null)
            foreach (NoteInfo n in notes)
                duration = Mathf.Max(duration, n.EndTime);
        return duration;
    }

    static public NoteInfo[] OrderNotes(NoteInfo[] notes, bool byStartTime = true, bool timeReverse = false, bool byTone = true, bool toneReverse = false)
    {
        if (notes == null) return null;
        NoteComparer comparer = new NoteComparer()
        { 
            compareStartTime = byStartTime, 
            compareTone = byTone, 
            invertTime = timeReverse, 
            invertTone = toneReverse 
        };
        return notes.OrderBy(n => n, comparer).ToArray();
    }

    static public int[] OrderNoteIndices(int[] noteIndices, NoteInfo[] notes, bool byStartTime = true, bool timeReverse = false, bool byTone = true, bool toneReverse = false)
    {
        if (notes == null || noteIndices == null) return null;
        int indexCount = noteIndices.Length;
        for (int i = 0; i < indexCount; i++) noteIndices[i] = i;
        NoteComparer comparer = new NoteComparer()
        { 
            compareStartTime = byStartTime,
            compareTone = byTone,
            invertTime = timeReverse,
            invertTone = toneReverse
        };
        return noteIndices.OrderBy(i => notes[i], comparer).ToArray();
    }

    static public NoteInfo[] Assemble(params NoteInfo[][] notes)
    {
        if (notes == null) return null;
        // Get total note count
        int totalNoteCount = 0;
        foreach (NoteInfo[] notePack in notes)
            if (notePack != null) totalNoteCount += notePack.Length;
        // Create array containing all notes
        NoteInfo[] assembled = new NoteInfo[totalNoteCount];
        int copyAt = 0;
        foreach (NoteInfo[] notePack in notes)
        {
            if (notePack != null)
            {
                Array.Copy(notePack, 0, assembled, copyAt, notePack.Length);
                copyAt += notePack.Length;
            }
        }
        // Order notes and return result
        return OrderNotes(assembled);

    }

    static public int[] Assemble(NoteInfo[] notes, params int[][] noteIndices)
    {
        if (notes == null) return null;
        // Get total note count
        int totalNoteCount = 0;
        foreach (int[] notePack in noteIndices)
            if (notePack != null) totalNoteCount += notePack.Length;
        // Create array containing all notes
        int[] assembled = new int[totalNoteCount];
        int copyAt = 0;
        foreach (int[] notePack in noteIndices)
        {
            if (notePack != null)
            {
                Array.Copy(notePack, 0, assembled, copyAt, notePack.Length);
                copyAt += notePack.Length;
            }
        }
        // Order notes and return result
        return OrderNoteIndices(assembled, notes);
    }

    public override string ToString()
    {
        return "Note " + tone + " starting at " + startTime + "s during " + duration + "s ";
    }
}