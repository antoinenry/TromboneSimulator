public interface ITromboneOutput { }
public interface ITromboneGrabOutput : ITromboneOutput { public bool? Grab { get; } }
public interface ITromboneBlowOutput : ITromboneOutput { public bool? Blow { get; } }
public interface ITromboneSlideToneOutput : ITromboneOutput { public float? SlideTone { get; } }
public interface ITrombonePressureLevelOutput : ITromboneOutput { public float? PressureLevel { get; } }
public interface ITrombonePressureToneOutput : ITromboneOutput { public float? PressureTone { get; } }

public interface ITromboneInput { public void ClearInputs(); }
public interface ITromboneGrabInput : ITromboneInput { public bool? Grab { set; } }
public interface ITromboneBlowInput : ITromboneInput { public bool? Blow { set; } }
public interface ITromboneSlideToneInput : ITromboneInput { public float? SlideTone { set; } }
public interface ITrombonePressureLevelInput : ITromboneInput { public float? PressureLevel { set; } }
public interface ITrombonePressureToneInput : ITromboneInput { public float? PressureTone { set; } }