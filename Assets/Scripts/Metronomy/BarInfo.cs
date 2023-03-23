using System;

[Serializable]
public struct BarInfo
{
    public int index;
    public float startTime;
    public int startBeatIndex;
    public float durationInSeconds;
    public int durationInBeats;

    static public BarInfo NaN => new BarInfo()
    {
        index = -1,
        startTime = float.NaN,
        startBeatIndex = -1,
        durationInSeconds = float.NaN,
        durationInBeats = -1
    };
}