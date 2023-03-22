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
}