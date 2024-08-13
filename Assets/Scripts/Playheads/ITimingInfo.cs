using UnityEngine;
using System.Collections.Generic;

public interface ITimingInfo
{
    public float StartTime { get; set; }
    public float Duration { get; set; }
    public float EndTime { get; set; }

    public static ITimingInfo None { get; }
}

public class ITimingInfoComparer : IComparer<ITimingInfo>
{
    public bool invertTime;

    public int Compare(ITimingInfo x, ITimingInfo y)
    {
        if (x.StartTime > y.StartTime) return invertTime ? -1 : 1;
        if (x.StartTime < y.StartTime) return invertTime ? 1 : -1;
        return 0;
    }
}