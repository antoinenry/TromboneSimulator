using System;
using UnityEngine;

[Serializable]
public struct MeasureInfo
{
    public int bar;
    public int timeSignatureNumerator;
    public int timeSignatureDenominator;
    public float quarterNotesPerBeat;

    public float QuarterNotesPerBar => timeSignatureDenominator != 0 ? 4f * timeSignatureNumerator / timeSignatureDenominator : 0f;
    public int BeatsPerBar
    {
        get
        {
            float beatsPerBar = QuarterNotesPerBar != 0f ? QuarterNotesPerBar / quarterNotesPerBeat : 0;
            if (beatsPerBar != Mathf.Floor(beatsPerBar))
                Debug.LogWarning("QuarterNotePerBar (" + QuarterNotesPerBar + ") i no multiple of quarterNotesPerBeat (" + quarterNotesPerBeat + "). This will cause approximations and time errors.");
            return (int)beatsPerBar;
        }
    }

    public MeasureInfo(int atBar)
    {
        bar = atBar;
        timeSignatureNumerator = 4;
        timeSignatureDenominator = 4;
        quarterNotesPerBeat = 1f;
    }

    public MeasureInfo(int atBar, int numerator, int denominator, float qNotesParBeat = 0)
    {
        bar = atBar;
        timeSignatureNumerator = numerator;
        timeSignatureDenominator = denominator;
        quarterNotesPerBeat = qNotesParBeat > 0 ? qNotesParBeat : DefaultQuarterNotePerBeat(numerator, denominator);
    }

    static public float DefaultQuarterNotePerBeat(int numerator, int denominator)
    {
        if (denominator == 0) return 0f;
        // Exception for standard ternary signatures
        if (denominator == 8) return 1.5f;
        // By default, use denominator value
        return 4f / denominator;
    }

    public float DefaultQuarterNotePerBeat() => DefaultQuarterNotePerBeat(timeSignatureNumerator, timeSignatureDenominator);
}
