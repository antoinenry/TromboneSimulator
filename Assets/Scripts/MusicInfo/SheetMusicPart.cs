using System;
using System.Linq;

[Serializable]
public struct SheetMusicPart : IEquatable<SheetMusicPart>
{
    public string name;
    public NoteInfo[] notes;

    public int NoteCount => notes != null ? notes.Length : 0;

    public SheetMusicPart Transpose(float byTones)
    {
        SheetMusicPart newPart = new();
        newPart.name = name;
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