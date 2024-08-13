using System;
using UnityEngine;

public abstract class LevelEventSheet : ScriptableObject
{
    public abstract Type EventInfoType { get; }
    public abstract Type EventInstanceType { get; }

    public abstract int EventCount { get; }
    public abstract ITimingInfo[] GetEvents();
}

public abstract class LevelEventSheet<T> : LevelEventSheet where T : ITimingInfo
{
    public override Type EventInfoType => typeof(T);
}
