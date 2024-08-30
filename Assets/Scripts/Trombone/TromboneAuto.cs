using UnityEngine;

public class TromboneAuto : MonoBehaviour,
    ITromboneGrabInput, ITromboneBlowInput, ITromboneSlideToneInput, ITrombonePressureLevelInput,
    ITromboneBlowOutput, ITromboneSlideToneOutput, ITrombonePressureLevelOutput
{
    [Header("Components")]
    public Playhead<INoteInfo> playhead;
    public NoteSpawner spawner;
    [Header("Controls")]
    public TromboneAutoSettings autoSettings;

    private float autoTone;
    private bool autoBlow;
    private float autoSlideTone;
    private float autoPressureLevel;
    private float legatoSlideTone;
    private float lockedPressureLevel;

    private bool? grabInput;
    private bool? blowInput;
    private float? slideToneInput;
    private float? pressureLevelInput;

    public Playhead<INoteInfo> ActivePlayhead { get; private set; }

    #region Input/Output interfaces
    public bool? Grab { set => grabInput = value; }

    public bool? Blow
    {
        set => blowInput = value;
        get
        {
            // Grab/Release contitions
            bool grabConditions = 
                (grabInput == true && autoSettings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Grabbed))
                || (grabInput == false && autoSettings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Released));
            // Control blows
            if (autoSettings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Blows) && autoBlow == true)
                return grabConditions ? true : null;
            // Control silences
            if (autoSettings.blowControl.HasFlag(TromboneAutoSettings.ControlConditions.Silences) && autoBlow == false)
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
            // Slide control
            bool grabConditions, blowConditions;
            if (autoSettings.slideControl != TromboneAutoSettings.ControlConditions.Never)
            {
                grabConditions =
                    (autoSettings.slideControl.HasFlag(TromboneAutoSettings.ControlConditions.Grabbed) && grabInput == true)
                        || (autoSettings.slideControl.HasFlag(TromboneAutoSettings.ControlConditions.Released) && grabInput == false);
                blowConditions =
                    (autoSettings.slideControl.HasFlag(TromboneAutoSettings.ControlConditions.Blows) && autoBlow == true)
                        || (autoSettings.slideControl.HasFlag(TromboneAutoSettings.ControlConditions.Silences) && autoBlow == false);
                if (grabConditions == true && blowConditions == true)
                {
                    // Set slide to automatic value
                    if (autoSettings.slideLegatoSpeed <= 0f)
                    {
                        return autoSlideTone;
                    }
                    // ..with legato effect
                    else
                    {
                        float distance = Mathf.Abs(legatoSlideTone - autoSlideTone);
                        if (distance > autoSettings.maxLegatoDistance)
                            legatoSlideTone = legatoSlideTone < autoSlideTone ? autoSlideTone - autoSettings.maxLegatoDistance : autoSlideTone + autoSettings.maxLegatoDistance;
                        else
                            legatoSlideTone = Mathf.MoveTowards(legatoSlideTone, autoSlideTone, Time.deltaTime * autoSettings.slideLegatoSpeed);
                        return legatoSlideTone;
                    }
                }
            }
            // No controls
            return slideToneInput;
        }
    }

    public float? PressureLevel 
    {
        set => pressureLevelInput = value;
        get
        {
            // Pressure control
            bool grabConditions, blowConditions;
            if (autoSettings.pressureControl != TromboneAutoSettings.ControlConditions.Never)
            {
                grabConditions =
                    ((autoSettings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Grabbed) && grabInput == true)
                        || (autoSettings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Released) && grabInput == false));
                blowConditions =
                    ((autoSettings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Blows) && autoBlow == true)
                        || (autoSettings.pressureControl.HasFlag(TromboneAutoSettings.ControlConditions.Silences) && autoBlow == false));
                if (grabConditions == true && blowConditions == true)
                {
                    // Set pressure to automatic value
                    return autoPressureLevel;
                }
            }
            // Pressure lock
            bool toneCondition;
            if (autoSettings.pressureLock != TromboneAutoSettings.LockConditions.Never)
            {
                blowConditions =
                    ((autoSettings.pressureLock.HasFlag(TromboneAutoSettings.LockConditions.InputBlows) && blowInput == true)
                        || (autoSettings.pressureLock.HasFlag(TromboneAutoSettings.LockConditions.AutoBlows) && autoBlow == true));
                toneCondition =
                    (autoSettings.pressureLock.HasFlag(TromboneAutoSettings.LockConditions.CorrectPressure) == false
                        || Mathf.RoundToInt(lockedPressureLevel) == Mathf.RoundToInt(autoPressureLevel));
                if (blowConditions && toneCondition)
                {
                    // Set pressure to registered (locked) value
                    return lockedPressureLevel;
                }
                // When pressure is "unlocked", registered value follows the input value
                else if (pressureLevelInput != null) lockedPressureLevel = Mathf.Round(pressureLevelInput.Value);
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

    private void SetActivePlayhead(Playhead<INoteInfo> setPlayhead)
    {
        if (ActivePlayhead != null)
        {
            ActivePlayhead.onStartEnterRead.RemoveListener(OnAutoPlayNoteEnter);
            ActivePlayhead.onRead.RemoveListener(OnAutoPlayNoteStay);
            ActivePlayhead.onEndExitRead.RemoveListener(OnAutoPlayNoteExit);
            ActivePlayhead.onPause.RemoveListener(OnAutoPlayPause);
            ActivePlayhead.onStop.RemoveListener(OnAutoPlayStop);
        }
        if (setPlayhead != null)
        {
            setPlayhead.onStartEnterRead.AddListener(OnAutoPlayNoteEnter);
            setPlayhead.onRead.AddListener(OnAutoPlayNoteStay);
            setPlayhead.onEndExitRead.AddListener(OnAutoPlayNoteExit);
            setPlayhead.onPause.AddListener(OnAutoPlayPause);
            setPlayhead.onStop.AddListener(OnAutoPlayStop);
        }
        ActivePlayhead = setPlayhead;
    }

    private void StartNote(INoteInfo note, int noteIndex)
    {
        // Start note
        if (note != null)
        {
            autoTone = note.Tone;
            Vector2 noteCoordinate = spawner.GetNotePlacement(note, noteIndex);
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
        autoBlow = false;
        //autoTone = float.NaN;
        //autoSlideTone = float.NaN;
        //autoPressureLevel = float.NaN;
    }

    private void StopNote(INoteInfo note)
    {
        // Stop current note
        if (note != null && note.Tone == autoTone) StopNote();
    }

    private void OnAutoPlayNoteEnter(int noteIndex, INoteInfo note)
    {
        StartNote(note, noteIndex);
    }

    private void OnAutoPlayNoteStay(int noteIndex, INoteInfo note)
    {
        // If note currently playing, start now
        if (autoBlow == false) StartNote(note, noteIndex);
        // Keep blowing as long as playhead is moving
        HoldNote(note != null && note.Tone == autoTone && playhead.DeltaTime != 0f);
    }

    private void OnAutoPlayNoteExit(int noteIndex, INoteInfo note)
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