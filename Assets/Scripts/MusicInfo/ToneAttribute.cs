using UnityEngine;

public class ToneAttribute : PropertyAttribute
{
    public bool hideDrumHit = false;

    public ToneAttribute(bool drumHit = false) { hideDrumHit = !drumHit; }

    static public int C0Tone => 12;
    static public int A0Tone => C0Tone + 9;
    static public char SharpSymbol => '#';
    static public string MicroToneSeparator => " +";
    static public string DrumhitSeparator => "  ";
    static public int MaxOctave => 5;

    static public string GetNoteName(int noteNumber, int octaveNumber, float microtones = 0f, bool hideDrumHit = true)
    {
        if (noteNumber < 0 || noteNumber > 11) return null;
        // Construct name string
        string noteName = string.Empty;
        // Note letter
        noteName += GetNoteLetter(noteNumber);
        // Sharp
        if (IsNatural(noteNumber) == false) noteName += SharpSymbol;
        // Negative octave
        if (octaveNumber < 0) noteName += '-';
        // Octave number
        noteName += (char)('0' + Mathf.Abs(octaveNumber));
        // Microtones
        if (microtones != 0f) noteName += MicroToneSeparator + microtones.ToString();
        // Drum hit
        if (hideDrumHit == false)
        {
            float tone = GetNoteTone(noteNumber, octaveNumber);
            if (InstrumentDictionary.FindCurrentDrumHitName(tone, out string drumHit)) noteName += DrumhitSeparator + drumHit;
        }
        return new string(noteName);
    }

    static public string GetNoteName(float tone, bool hideDrumHit = true)
    {
        int intTone = Mathf.FloorToInt(tone);
        float microtones = tone - intTone;
        GetNoteAndOctave(intTone, out int noteNumber, out int octaveNumber);
        return GetNoteName(noteNumber, octaveNumber, microtones, hideDrumHit);
    }

    static public string GetNoteName(int tone, bool hideDrumHit = true)
    {
        GetNoteAndOctave(tone, out int noteNumber, out int octaveNumber);
        return GetNoteName(noteNumber, octaveNumber, 0f, hideDrumHit);
    }

    static public void GetNoteAndOctave(int tone, out int noteNumber, out int octaveNumber)
    {
        octaveNumber = (tone - C0Tone) / 12;
        noteNumber = (int)Mathf.Repeat(tone - A0Tone, 12);
    }

    static public float GetNoteTone(string noteName)
    {
        if (noteName == null) return -1f;
        int octaveNumber = -1;
        int noteNumber = -1;
        float microTones = 0f;
        // Parse drumhit name
        string[] splitName = noteName.Split(DrumhitSeparator);
        if (splitName.Length > 2) 
            return float.NaN;
        else if (splitName.Length == 2)
        {
            noteName = splitName[0];
        }
        // Parse microtones
        splitName = noteName.Split(MicroToneSeparator);
        if (splitName.Length > 2) 
            return float.NaN;
        else if (splitName.Length == 2)
        {
            if (float.TryParse(splitName[1], out microTones))
                noteName = splitName[0];
            else
                return -1f;
        }
        // Natural note
        if (noteName.Length == 2)
        {
            noteNumber = GetNoteNumber(noteName[0]);
            octaveNumber = noteName[1] - '0';
        }
        // Sharp or negative note
        if (noteName.Length == 3)
        {
            // Sharp
            if (noteName[1] == SharpSymbol)
            {
                noteNumber = GetNoteNumber(noteName[0]) + 1;
                octaveNumber = noteName[2] - '0';
            }
            // Negative
            else if (noteName[1] == '-')
            {
                noteNumber = GetNoteNumber(noteName[0]);
                octaveNumber = -(noteName[2] - '0');
            }
        }
        // Sharp negative note
        else if (noteName.Length == 4 && noteName[2] == SharpSymbol && noteName[3] == '-')
        {
            noteNumber = GetNoteNumber(noteName[0]) + 1;
            octaveNumber = -(noteName[3] - '0');

        }
        return GetNoteTone(noteNumber, octaveNumber) + microTones;
    }

    static public float GetNoteTone(int noteNumber, int octaveNumber)
    {
        if (noteNumber >= 0 && noteNumber < 12) return C0Tone + octaveNumber * 12 + (noteNumber+ 9) % 12;
        else return float.NaN;
    }

    static public bool IsNatural(int noteNumber)
    {
        if (noteNumber < 3 || noteNumber > 7) return noteNumber % 2 == 0;
        else return noteNumber % 2 == 1;
    }

    static public char GetNoteLetter(int noteNumber)
    {
        if (noteNumber < 3) return (char)('A' + (noteNumber / 2));
        else if (noteNumber < 8) return (char)('A' + ((noteNumber + 1) / 2));
        else return (char)('A' + ((noteNumber + 2) / 2));
    }

    static public int GetNoteNumber(char noteLetter)
    {
        if (noteLetter < 'A' || noteLetter > 'G') return -1;
        else if (noteLetter < 'C') return (noteLetter - 'A') * 2;
        else if (noteLetter < 'F') return (noteLetter - 'A') * 2 - 1;
        else return (noteLetter - 'A') * 2 - 2;
    }

    static public string[] GetAllNoteNames()
    {
        return GetAllNoteNames(MaxOctave + 1);
    }

    static public string[] GetAllNoteNames(int octaves)
    {
        if (octaves <= 0) return new string[0];
        int noteCount = (octaves + 1) * 12;
        string[] allNotes = new string[noteCount];
        for (int t = 0; t < noteCount; t++)
        {
            allNotes[t] = GetNoteName(C0Tone + t);
        }
        return allNotes;
    }
}
