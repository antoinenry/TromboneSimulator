using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct NoteInfo : INoteInfo
{
    [Tone] public float tone;
    [Range(0f, 1f)] public float velocity;
    public float startTime;
    public float duration;

    #region Interface Infos
    public float Tone { get => tone; set => tone = value; }
    public float Velocity { get => velocity; set => velocity = value; }
    public float StartTime { get => startTime; set => startTime = value; }
    public float Duration { get => duration; set => duration = value; }
    public float EndTime { get => startTime + duration; set => duration = Mathf.Max(0f, value - startTime); }
    #endregion

    static public NoteInfo None => new NoteInfo()
    {
        tone = 0f,
        velocity = 0f,
        startTime = 0f,
        duration = 0f
    };

    static public NoteInfo GetInfo(INoteInfo note)
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

    static public void SetInfo(INoteInfo note, NoteInfo info)
    {
        if (note != null)
        {
            note.Tone = info.tone;
            note.Velocity = info.velocity;
            note.StartTime = info.startTime;
            note.Duration = info.duration;
        }
    }

    public override bool Equals(object obj) => Equals((NoteInfo)obj);

    public override int GetHashCode() => base.GetHashCode();

    public bool Equals(NoteInfo other)
        => tone == other.tone && velocity == other.velocity && startTime == other.startTime && duration == other.duration;

    public static bool operator ==(NoteInfo left, NoteInfo right) => left.Equals(right);
    public static bool operator !=(NoteInfo left, NoteInfo right) => !left.Equals(right);

    public void Copy(NoteInfo copyFrom)
    {
        tone = copyFrom.tone;
        velocity = copyFrom.velocity;
        startTime = copyFrom.startTime;
        duration = copyFrom.duration;
    }

    public NoteInfo ScaleTime(float scale)
    {
        NoteInfo newNote = new NoteInfo();
        newNote.tone = tone;
        newNote.velocity = velocity;
        newNote.startTime = startTime * scale;
        newNote.duration = duration * scale;
        return newNote;
    }

    public static NoteInfo[] ScaleTime(NoteInfo[] notes, float scale)
        => notes != null ? Array.ConvertAll(notes, n => n.ScaleTime(scale)) : null;

    public NoteInfo Transpose(float byTones)
    {
        NoteInfo newNote = new NoteInfo();
        newNote.tone = tone + byTones;
        newNote.velocity = velocity;
        newNote.startTime = startTime;
        newNote.duration = duration;
        return newNote;
    }

    public static NoteInfo[] Transpose(NoteInfo[] notes, float byTones)
        => notes != null ? Array.ConvertAll(notes, n => n.Transpose(byTones)) : null;

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
        NoteInfoComparer comparer = new NoteInfoComparer()
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
        NoteInfoComparer comparer = new NoteInfoComparer()
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