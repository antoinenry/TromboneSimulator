using System.Reflection;
using UnityEngine;

public abstract class LevelEventInstance : MonoBehaviour, ITimingInfo
{
    public abstract ITimingInfo TimingInfo { get; set; }
    public float StartTime { get => TimingInfo.StartTime; set => TimingInfo.StartTime = value; }
    public float Duration { get => TimingInfo.Duration; set => TimingInfo.Duration = value; }
    public float EndTime { get => StartTime + Duration; set => Duration = Mathf.Max(0f, value - StartTime); }

    public abstract void SetEventInfo(ITimingInfo info);
    public virtual void StartEvent() => enabled = true;
    public virtual void EndEvent() => enabled = false;
}

public abstract class LevelEventInstance<T> : LevelEventInstance where T : ITimingInfo
{
    public virtual T EventInfo { get; set; }

    public override void SetEventInfo(ITimingInfo info)
    {
        if (info is T) EventInfo = (T)info;
        else TimingInfo = info;
    }
}