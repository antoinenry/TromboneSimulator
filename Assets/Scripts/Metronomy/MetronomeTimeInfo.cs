using System;

[Serializable]
public struct MetronomeTimeInfo
{
    public int beatIndex;
    public float beatTime;
    public float beatDuration;

    public int barIndex;
    public float barTime;
    public float nextBarTime;
}
