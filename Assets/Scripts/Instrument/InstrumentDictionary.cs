using UnityEngine;
using System;

[ExecuteAlways]
[CreateAssetMenu(fileName = "NewInstrumentDictionary", menuName = "Trombone Hero/Instruments/Instrument Dictionary")]
public class InstrumentDictionary : ScriptableObject
{
    [Serializable]
    public struct InstrumentInfo
    {
        public string officialName;
        public string[] alternativeNames;
        public bool isDrums;
        [Tone] public float lowestTone;
        [Tone] public float highestTone;
    }

    [Serializable]
    public struct DrumHitInfo
    {
        public string name;
        [Tone] public float[] tones;
    }

    public bool isCurrent;
    public InstrumentInfo[] instruments;
    public DrumHitInfo[] drumHits;

    static public InstrumentDictionary Current { get; private set; }

    private void OnEnable()
    {
        if (isCurrent)
        {
            if (Current != null) Debug.LogWarning("Several InstrumentDictionnaries are set as current. Ignoring " + this + ".");
            else Current = this;
        }
    }

    static public bool FindCurrentOfficalName(string instrumentName, out string officialName)
    {
        InstrumentDictionary current = Current;
        if (current != null) return current.FindOfficalName(instrumentName, out officialName);
        else
        {
            officialName = null;
            Debug.LogWarning("Not current instrument dictionnary");
            return false;
        }
    }

    static public bool IsCurrentDrums(string instrumentName)
    {
        InstrumentDictionary current = Current;
        if (current != null) return current.IsDrums(instrumentName);
        else
        {
            Debug.LogWarning("Not current instrument dictionnary");
            return false;
        }
    }

    static public bool FindCurrentDrumHitName(float tone, out string hitName)
    {
        InstrumentDictionary current = Current;
        if (current != null) return current.FindDrumHitName(tone, out hitName);
        else
        {
            hitName = null;
            Debug.LogWarning("Not current instrument dictionnary");
            return false;
        }
    }

    static public bool FindCurrentAlternativeDrumHitTones(float tone, out float[] alternatives)
    {
        InstrumentDictionary current = Current;
        if (current != null) return current.FindAlternativeDrumHitTones(tone, out alternatives);
        else
        {
            alternatives = null;
            Debug.LogWarning("Not current instrument dictionnary");
            return false;
        }
    }

    static public bool CheckCurrentToneRange(float tone, string instrumentName)
    {
        InstrumentDictionary current = Current;
        if (current != null) return current.CheckToneRange(tone, instrumentName);
        else
        {            
            Debug.LogWarning("Not current instrument dictionnary");
            return false;
        }
    }

    static public bool SameCurrentInstruments(string name1, string name2)
    {
        InstrumentDictionary current = Current;
        if (current != null) return current.SameInstruments(name1, name2);
        else
        {
            Debug.LogWarning("Not current instrument dictionnary");
            return false;
        }
    }

    public bool FindOfficalName(string instrumentName, out string officialName)
    {
        int instrumentIndex = FindInstrumentIndex(instrumentName);
        if (instrumentIndex != -1)
        {
            officialName = instruments[instrumentIndex].officialName;
            return true;
        }
        else
        {
            officialName = null;
            return false;
        }
    }

    public bool IsDrums(string instrumentName)
    {
        int instrumentIndex = FindInstrumentIndex(instrumentName);
        if (instrumentIndex != -1) return instruments[instrumentIndex].isDrums;
        else return false;
    }

    public bool FindDrumHitName(float tone, out string hitName)
    {
        if (drumHits != null)
        {
            int hitIndex = Array.FindIndex(drumHits, h => h.tones != null && Array.IndexOf(h.tones, tone) != -1);
            if (hitIndex != -1)
            {
                hitName = drumHits[hitIndex].name;
                return true;
            }
        }
        hitName = null;
        return false;
    }

    public bool FindAlternativeDrumHitTones(float tone, out float[] alternatives)
    {
        if (drumHits != null)
        {
            int hitIndex = Array.FindIndex(drumHits, h => h.tones != null && Array.IndexOf(h.tones, tone) != -1);
            if (hitIndex != -1)
            {
                alternatives = drumHits[hitIndex].tones;
                return true;
            }
        }
        alternatives = null;
        return false;
    }

    public bool CheckToneRange(float tone, string instrumentName)
    {
        int instrumentIndex = FindInstrumentIndex(instrumentName);
        return instrumentIndex != -1 
            && tone >= instruments[instrumentIndex].lowestTone 
            && tone <= instruments[instrumentIndex].highestTone;
    }

    public bool SameInstruments(string name1, string name2)
    {
        if (FindOfficalName(name1, out string official1) && FindOfficalName(name2, out string official2))
            return official1 == official2;
        else
            return false;
    }

    private int FindInstrumentIndex(string instrumentName)
    {
        if (instrumentName == null) return -1;
        int instrumentCount = instruments != null ? instruments.Length : 0;
        for (int i = 0; i < instrumentCount; i++)
        {
            InstrumentInfo instrumentNames = instruments[i];
            if (instrumentName.Contains(instrumentNames.officialName, StringComparison.InvariantCultureIgnoreCase))
            {
                return i;
            }
            foreach (string s in instrumentNames.alternativeNames)
                if (instrumentName.Contains(s, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
        }
        return -1;
    }
}