using UnityEngine;
using System;

[Serializable]
public struct InstrumentMixInfo
{
    public string partName;
    public SamplerInstrument instrument;
    [Range(0f, 1f)] public float volume;
    [Range(-1f, 1f)] public float pan;

    public string Name => instrument != null ? instrument.instrumentName : null;

    public string OfficialInstrumentName
    {
        get
        {
            if (InstrumentDictionary.Current.FindOfficalName(Name, out string officialName) == true)
                return officialName;
            else
                return Name;
        }
    }
}