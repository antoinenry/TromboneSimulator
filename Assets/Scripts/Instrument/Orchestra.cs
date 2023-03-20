using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrchestra", menuName = "Trombone Hero/Instruments/Orchestra")]
public class Orchestra : ScriptableObject
{
    public InstrumentMixInfo[] instrumentInfo;

    public SamplerInstrument[] Instrument => (instrumentInfo != null) ? Array.ConvertAll(instrumentInfo, p => p.instrument) : null;
}
