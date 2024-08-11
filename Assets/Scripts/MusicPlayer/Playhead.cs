using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class Playhead : ScriptableObject
{
    [Flags]
    public enum ReadProgress
    {
        None = 0,
        IsBeforeTime = 1,
        StartsEnteringRead = 2,
        IsEnteringRead = 4,
        EndsEnteringRead = 8,
        IsOnTime = 16,
        StartsExitingRead = 32,
        IsExitingRead = 64,
        EndsExitingRead = 128,
        IsAfterTime = 256
    }

    public bool showDebug = false;
    [Header("Timing")]
    public float timeOffset = 0f;
    public float timeWidth = 0f;
    [Header("Loop")]
    public bool loop = false;
    public float loopStart = 0f;
    public float loopEnd = 0f;
    public int playedLoopCount = 0;
    [Header("Events")]
    public UnityEvent<float, float> onMove;
    public UnityEvent<float> onPause;
    public UnityEvent onStop;

    public float PreviousTime { get; protected set; }
    public float CurrentTime { get; protected set; }
    public float DeltaTime { get; protected set; }
    public float PlayingSpeed { get; protected set; }
    public float LoopWidth => loopEnd - loopStart;

    public float MinimumTime
    {
        get
        {
            return timeOffset - timeWidth / 2f;
        }
        set
        {
            timeWidth = MaximumTime - value;
            timeOffset = value + timeWidth / 2f;
        }
    }

    public float MaximumTime
    {
        get
        {
            return timeOffset + timeWidth / 2f;
        }
        set
        {
            timeWidth = value - MinimumTime;
            timeOffset = value - timeWidth / 2f;
        }
    }


    public virtual float LoopTime(float t)
    {
        return Mathf.Repeat(t - loopStart, loopEnd - loopStart) + loopStart;
    }

    public virtual void Stop()
    {
        onStop.Invoke();
    }

    public abstract void Clear();
}