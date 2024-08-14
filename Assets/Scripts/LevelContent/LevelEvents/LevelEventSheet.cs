using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class LevelEventSheet : ScriptableObject
{
    public abstract Type EventInfoType { get; }
    public abstract Type EventInstanceType { get; }

    public abstract int EventCount { get; }
    public abstract ITimingInfo GetEventTiming(int index);
    public abstract ITimingInfo[] GetEventTimings();
    public abstract void SetEventTiming(int eventIndex, ITimingInfo timingInfo);

    public void MultiplyTempoBy(float tempoModifier)
    {
        if (tempoModifier == 0f || tempoModifier == 1f) return;
        float timeScale = 1f / tempoModifier;
        for (int i = 0, iend = EventCount; i < iend; i++)
        {
            ITimingInfo getEvent = GetEventTiming(i);
            getEvent.StartTime *= timeScale;
            getEvent.Duration *= timeScale;
            SetEventTiming(i, getEvent);
        }
    }
}

public abstract class LevelEventSheet<T> : LevelEventSheet where T : ITimingInfo
{
    public override Type EventInfoType => typeof(T);

    public override ITimingInfo GetEventTiming(int index) => GetEvent(index);

    public override ITimingInfo[] GetEventTimings() => Array.ConvertAll(GetEvents(), e => (ITimingInfo)e);

    public override void SetEventTiming(int eventIndex, ITimingInfo timingInfo)
    {
        T getEvent = GetEvent(eventIndex);
        getEvent.StartTime = timingInfo.StartTime;
        getEvent.Duration = timingInfo.Duration;
        SetEvent(eventIndex, getEvent);
    }

    public abstract T[] GetEvents();
    public abstract T GetEvent(int eventIndex);
    public abstract void SetEvent(int eventIndex, T eventInfo);
}
