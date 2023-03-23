using System;

[Serializable]
public struct BeatInfo
{
    public int index;
    public float startTime;
    public float duration;

    static public BeatInfo NaN => new BeatInfo()
    {
        index = -1,
        startTime = float.NaN,
        duration = float.NaN
    };
}