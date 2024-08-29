using Melanchall.DryWetMidi.Interaction;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.SocialPlatforms;

[Serializable]
public struct TempoInfo
{
    public float time;
    public float secondsPerQuarterNote;

    public TempoInfo(float atTime)
    {
        time = atTime;
        secondsPerQuarterNote = 1f;
    }

    public TempoInfo(float atTime, float secPerQuarterNote)
    {
        time = atTime;
        secondsPerQuarterNote = secPerQuarterNote;
    }

    public TempoInfo ScaleTime(float timeScale) 
        => new TempoInfo(time * timeScale, secondsPerQuarterNote * timeScale);

    public static TempoInfo[] ScaleTime(TempoInfo[] tempos, float timeScale)
     => tempos != null ? Array.ConvertAll(tempos, t => t.ScaleTime(timeScale)) : null;

    public static void MultiplyTempoBy(ref TempoInfo[] tempoInfos, float tempoModifier)
    {
        if (tempoModifier == 0f || tempoModifier == 1f) return;
        float timeScale = 1f / tempoModifier;
        for (int i = 0, iend  = tempoInfos.Length; i < iend; i++)
            tempoInfos[i] = tempoInfos[i].ScaleTime(timeScale);
    }
}