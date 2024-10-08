using System;
using System.Linq;

[Serializable]
public struct SheetMusicPart : IEquatable<SheetMusicPart>
{
    public string name;
    public NoteInfo[] notes;

    public int NoteCount => notes != null ? notes.Length : 0;

    public NoteInfo GetNote(int index)
    {
        if (index < 0 || index >= NoteCount) return NoteInfo.None;
        return notes[index];
    }

    public void SetNote(int index, NoteInfo note)
    {
        if (index < 0 || index >= NoteCount) return;
        notes[index] = note;
    }

    public SheetMusicPart Transpose(float byTones, bool transposeDrums = false)
    {
        SheetMusicPart newPart = new();
        newPart.name = name;
        if (InstrumentDictionary.IsCurrentDrums(name) && transposeDrums == false)
            newPart.notes = NoteInfo.Transpose(notes, 0f);
        else
            newPart.notes = NoteInfo.Transpose(notes, byTones);
        return newPart;
    }

    public SheetMusicPart ScaleTime(float scale)
    {
        SheetMusicPart newPart = new();
        newPart.name = name;
        newPart.notes = NoteInfo.ScaleTime(notes, scale);
        return newPart;
    }

    public bool Equals(SheetMusicPart other)
        => name == other.name 
        && ((notes == null && other.notes == null) || Enumerable.SequenceEqual(notes, other.notes));

    public static SheetMusicPart Merge(params SheetMusicPart[] parts)
    {
        SheetMusicPart newPart = new();
        if (parts == null || parts.Length == 0) return newPart;
        newPart.name = parts[0].name;
        NoteInfo[][] partNotes = Array.ConvertAll(parts, p => p.notes);
        newPart.notes = NoteInfo.Assemble(partNotes);
        return newPart;
    }
}