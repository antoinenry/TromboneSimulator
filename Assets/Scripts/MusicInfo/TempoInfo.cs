using System;

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

}