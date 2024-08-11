using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPerformanceStyle", menuName = "Trombone Hero/Performance Style")]
public class PerformanceStyle : ScriptableObject
{
    public bool useAsDefault;
    public float minimumNoteDuration = .075f;
    public float detachRepeatedNote = .05f;
    public float forceLegato = .01f;
    public AnimationCurve velocityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    static public PerformanceStyle Default;

    private void Awake()
    {
        if (useAsDefault) Default = this;
    }

    public void ProcessNotes(INoteInfo[] notes)
    {
        if (notes != null)
        {
            NoteInfo[] noteInfos = Array.ConvertAll(notes, n => NoteInfo.GetInfo(n));
            noteInfos = ProcessNotes(noteInfos);
            for (int n = 0, nCount = notes.Length; n < nCount; n++) NoteInfo.SetInfo(notes[n], noteInfos[n]);
        }
    }

    public NoteInfo[] ProcessNotes(NoteInfo[] notes)
    {
        if (notes == null) return null;
        int noteCount = notes.Length;
        NoteInfo[] processedNotes = new NoteInfo[noteCount];
        Array.Copy(notes, processedNotes, noteCount);
        ProcessNotes(ref processedNotes);
        return processedNotes;
    }

    public void ProcessNotes(ref NoteInfo[] notes)
    {
        if (notes != null)
        {
            //ForceLegato(ref notes, forceLegato);
            DetachRepeatedNotes(ref notes, detachRepeatedNote);
            ApplyMinimumNoteDuration(ref notes, minimumNoteDuration);
        }
    }

    static public void ForceLegato(ref NoteInfo[] notes, float closeSpace)
    {
        // Connect close notes to add more legato
        if (notes != null)
            for (int n = 0, nCount = notes.Length; n < nCount - 1; n++)
                if (notes[n + 1].startTime - notes[n].EndTime < closeSpace)
                    notes[n].EndTime = notes[n + 1].startTime;
    }

    static public void ApplyMinimumNoteDuration(ref NoteInfo[] notes, float minimum)
    {
        // Ensure every note is longer than minimum
        if (notes != null)
            for (int n = 0, nCount = notes.Length; n < nCount; n++)
                if (notes[n].duration < minimum)
                    notes[n].duration = minimum;
    }

    static public void DetachRepeatedNotes(ref NoteInfo[] notes, float minimumSpacing)
    {
        // Reduce duration of repeated note for better articulation
        if (notes != null)
            for (int n = 0, nCount = notes.Length; n < nCount - 1; n++)
                if (notes[n].tone == notes[n + 1].tone && notes[n + 1].startTime - notes[n].EndTime < minimumSpacing)
                    notes[n].EndTime = notes[n + 1].startTime - minimumSpacing;
    }
}
