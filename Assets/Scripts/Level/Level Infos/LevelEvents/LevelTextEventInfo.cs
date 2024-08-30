using System;
using UnityEngine;

[Serializable]
public struct LevelTextEventInfo : ITimingInfo
{
    public float startTime;
    public float duration;
    public string text;
    public bool danceAnimation;

    #region Interface Infos
    public float StartTime { get => startTime; set => startTime = value; }
    public float Duration { get => duration; set => duration = value; }
    public float EndTime { get => StartTime + Duration; set => Duration = Mathf.Max(0f, value - StartTime); }
    #endregion
}