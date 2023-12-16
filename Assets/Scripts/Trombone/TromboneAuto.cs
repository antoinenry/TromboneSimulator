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
            bool grabConditions = 
                (grabInput == true && settings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Grabbed))
                || (grabInput == false && settings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Released));
            // Control blows
            if (settings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Blows) && autoBlow == true)
                return grabConditions ? true : null;
            // Control silences
            if (settings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Silences) && autoBlow == false)
                return grabConditions ? false : null;
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
            bool grabConditions, blowConditions, toneCondition;
            // Pressure control
            if (settings.pressureControl != TromboneAutoSettings.ControlConditions.Never)
            {
                grabConditions =
                    ((settings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Grabbed) && grabInput == true)
                        || (settings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Released) && grabInput == false));
                blowConditions =
                    ((settings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Blows) && autoBlow == true)
                        || (settings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Silences) && autoBlow == false));
                if (grabConditions == true && blowConditions == true)
                {
                    // Set pressure to automatic value
                    return autoPressureLevel;
                }
            }
            // Pressure lock
            if (settings.pressureLock != TromboneAutoSettings.LockConditions.Never)
            {
                blowConditions =
                    ((settings.pressureLock.HasFlag(TromboneAutoSettings.LockConditions.InputBlows) && blowInput == true)
                        || (settings.pressureLock.HasFlag(TromboneAutoSettings.LockConditions.AutoBlows) && autoBlow == true));
                toneCondition =
                    (settings.pressureLock.HasFlag(TromboneAutoSettings.LockConditions.CorrectPressure) == false
                        || Mathf.RoundToInt(lockedPressureLevel) == Mathf.RoundToInt(autoPressureLevel));
                if (blowConditions && toneCondition)
                {
                    // Set pressure to registered (locked) value
                    return lockedPressureLevel;
                }
                // When pressure is "unlocked", registered value follows the input value
                else if (pressureLevelInput != null) lockedPressureLevel = pressureLevelInput.Value;
            }
            // No controls or lock
            return pressureLevelInput;
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