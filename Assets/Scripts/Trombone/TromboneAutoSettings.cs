using System;

[Serializable]
public struct TromboneAutoSettings
{
    [Flags] public enum ControlConditions
    {
        Never = 0,
        Grabbed = 1, Released = 2,
        Blows = 4, Silences = 8,
        Always = ~0
    }

    [Flags] public enum LockConditions
    {
        Never = 0,
        InputBlows = 4, AutoBlows = 8,
        CorrectPressure = 16 | AutoBlows,
        Always = InputBlows | AutoBlows,
    }

    public ControlConditions blowControl;
    public ControlConditions slideControl;
    public ControlConditions pressureControl;
    public LockConditions pressureLock;
}