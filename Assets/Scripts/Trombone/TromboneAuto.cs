using UnityEngine;

public class TromboneAuto : MonoBehaviour,
    ITromboneGrabInput, ITromboneBlowInput, ITromboneSlideToneInput, ITrombonePressureLevelInput,
    ITromboneBlowOutput, ITromboneSlideToneOutput, ITrombonePressureLevelOutput
{
    [Header("Components")]
    public Playhead playhead;
    public NoteSpawner spawner;
    [Header("Controls")]
    public TromboneAutoSettings settings;

    private float autoTone;
    private bool autoBlow;
    private float autoSlideTone;
    private float autoPressureLevel;
    private float lockedPressureLevel;

    private bool? grabInput;
    private bool? blowInput;
    private float? slideToneInput;
    private float? pressureLevelInput;

    public Playhead ActivePlayhead { get; private set; }

    #region Input/Output interfaces
    public bool? Grab { set => grabInput = value; }

    public bool? Blow
    {
        set => blowInput = value;
        get
        {
            // Grab/Release contitions
            bool controlConditions = 
                (grabInput == true && settings.blowConditions.HasFlag(TromboneAutoSettings.BlowConditions.OnGrabbed))
                || (grabInput == false && settings.blowConditions.HasFlag(TromboneAutoSettings.BlowConditions.OnReleased));
            // Control blows
            if (settings.blowControls.HasFlag(TromboneAutoSettings.BlowControls.ControlBlows) && autoBlow == true)
                return controlConditions ? true : null;            
            // Control silences
            if (settings.blowControls.HasFlag(TromboneAutoSettings.BlowControls.ControlSilences) && autoBlow == false)
                return controlConditions ? false : null;
            // No control
            return blowInput;
        }
    }

    public float? SlideTone
    {
        set => slideToneInput = value;
        get
        {
            return null;
        }
    }

    public float? PressureLevel 
    {
        set => pressureLevelInput = value;
        get
        {
            // No pressure control
            if (settings.pressureControls == TromboneAutoSettings.PressureControls.Nothing)
                return pressureLevelInput;
            // Check activation conditions
            bool control;
            control =
                   (settings.pressureConditions.HasFlag(TromboneAutoSettings.PressureConditions.OnAutoBlow) && autoBlow == true)
                || (settings.pressureConditions.HasFlag(TromboneAutoSettings.PressureConditions.OnInputBlow) && blowInput == true)
                || (settings.pressureConditions.HasFlag(TromboneAutoSettings.PressureConditions.OnAutoSilence) && autoBlow == false)
                || (settings.pressureConditions.HasFlag(TromboneAutoSettings.PressureConditions.OnInputSilence) && blowInput == false);
            // Special condition: correct pressure input
            if (settings.pressureConditions.HasFlag(TromboneAutoSettings.PressureConditions.OnCorrectInput) )
            {
                // Prevent control as long as pressure is incorrect
                if (Mathf.RoundToInt(lockedPressureLevel) != Mathf.RoundToInt(autoPressureLevel)) control = false;
            }
            // Pressure control
            if (control == true)
            {
                // Pressure lock
                if (settings.pressureControls == TromboneAutoSettings.PressureControls.LockPressure)
                    return lockedPressureLevel;
                // Full auto
                else
                    return autoPressureLevel;
            }
            // No pressure control
            else
            {
                // Change locked pressure only when conditions are not met
                if (settings.pressureControls == TromboneAutoSettings.PressureControls.LockPressure && pressureLevelInput != null)
                    lockedPressureLevel = pressureLevelInput.Value;
                // No controls means no change from input
                return pressureLevelInput;
            }
        }
    }

    #endregion

    private void OnEnable()
    {
        SetActivePlayhead(playhead);
        ClearInputs();
    }

    private void OnDisable()
    {
        SetActivePlayhead(null);
        StopNote();
        ClearInputs();
    }

    private void Update()
    {
        if (playhead != ActivePlayhead) SetActivePlayhead(playhead);
    }

    public void ClearInputs()
    {
        grabInput = null;
        blowInput = null;
        slideToneInput = null;
        pressureLevelInput = null;
    }

    private void SetActivePlayhead(Playhead setPlayhead)
    {
        if (ActivePlayhead != null)
        {
            ActivePlayhead.onStartEnterNote.RemoveListener(OnAutoPlayNoteEnter);
            ActivePlayhead.onNote.RemoveListener(OnAutoPlayNoteStay);
            ActivePlayhead.onEndExitNote.RemoveListener(OnAutoPlayNoteExit);
            ActivePlayhead.onPause.RemoveListener(OnAutoPlayPause);
            ActivePlayhead.onStop.RemoveListener(OnAutoPlayStop);
        }
        if (setPlayhead != null)
        {
            setPlayhead.onStartEnterNote.AddListener(OnAutoPlayNoteEnter);
            setPlayhead.onNote.AddListener(OnAutoPlayNoteStay);
            setPlayhead.onEndExitNote.AddListener(OnAutoPlayNoteExit);
            setPlayhead.onPause.AddListener(OnAutoPlayPause);
            setPlayhead.onStop.AddListener(OnAutoPlayStop);
        }
        ActivePlayhead = setPlayhead;
    }

    private void StartNote(INote note)
    {
        // Start note
        if (note != null)
        {
            autoTone = note.Tone;
            Vector2 noteCoordinate = spawner.GetNotePlacement(note);
            autoSlideTone = noteCoordinate.x;
            autoPressureLevel = noteCoordinate.y;
        }
    }

    private void HoldNote(bool on)
    {
        autoBlow = on;
    }

    private void StopNote()
    {
        // Stop any note
        //autoTone = float.NaN;
        autoBlow = false;
        //autoSlideTone = float.NaN;
        //autoPressureLevel = float.NaN;
    }

    private void StopNote(INote note)
    {
        // Stop current note
        if (note.Tone == autoTone) StopNote();
    }

    private void OnAutoPlayNoteEnter(int noteIndex, INote note)
    {
        StartNote(note);
    }

    private void OnAutoPlayNoteStay(int noteIndex, INote note)
    {
        // Keep blowing as long as plahead is moving
        HoldNote(playhead.DeltaTime != 0f);
    }

    private void OnAutoPlayNoteExit(int noteIndex, INote note)
    {
        // Stop blowing
        StopNote(note);
    }

    private void OnAutoPlayPause(float time)
    {
        // Stop blowing
        HoldNote(false);
    }

    private void OnAutoPlayStop()
    {
        // Stop blowing
        StopNote();
    }
}