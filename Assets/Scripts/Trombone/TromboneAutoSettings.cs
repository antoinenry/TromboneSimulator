using System;
using UnityEngine;

[Serializable]
public struct TromboneAutoSettings
{
    [Flags] public enum BlowControls 
    { 
        ControlBlows = 1, ControlSilences = 2, FullControl = ~0 
    }
    [Flags] public enum BlowConditions 
    { 
        Never = 0, OnGrabbed = 1, OnReleased = 2, Always =~0 
    }
    public enum PressureControls 
    { 
        Nothing = 0, LockPressure = 1, ControlPressure = 2
    }
    [Flags] public enum PressureConditions 
    { 
        Never = 0, 
        OnInputBlow = 1, OnInputSilence = 2, 
        OnAutoBlow = 4, OnAutoSilence = 8, 
        OnCorrectInput = 16,
        Always = ~0 
    }

    public BlowControls blowControls;
    public BlowConditions blowConditions;
    public PressureControls pressureControls;
    public PressureConditions pressureConditions;
    public float lockPressureRadius;
    public float unlockPressureRadius;
    public float lockedPressureLevel;
}