using UnityEngine;
using UnityEngine.Events;

public class TromboneCore : MonoBehaviour,
    ITromboneGrabInput, ITromboneBlowInput, ITromboneSlideToneInput, ITrombonePressureLevelInput, ITrombonePressureToneInput,
    ITromboneGrabOutput, ITromboneBlowOutput, ITromboneSlideToneOutput, ITrombonePressureLevelOutput, ITrombonePressureToneOutput
{
    public bool showDebug = false;
    [Header("Components")]
    public TromboneDisplay tromboneDisplay;
    public TromboneAudio tromboneAudio;
    public TromboneAuto tromboneAuto;
    [Header("Controls")]
    public bool grab;
    public bool blow;
    public float slideTone;
    public float pressureLevel;
    //public TromboneControlWiring controlWiring;
    [Header("Tone")]
    [Tone] public float baseTone;
    public float[] pressureStepTones;
    public float slideTones = 6f;

    public UnityEvent onChangeBuild;

    public int PressureIndex => RoundToPressureIndex(pressureLevel);
    public float Tone => GetTone(PressureIndex, slideTone);
    public float[] PressureTones
    {
        get
        {
            int stepCount = pressureStepTones == null ? 0 : pressureStepTones.Length;
            float[] tones = new float[stepCount + 1];
            tones[0] = baseTone;
            for (int p = 0; p < stepCount; p++) tones[p + 1] = tones[p] + pressureStepTones[p];
            return tones;
        }
    }
    public SamplerInstrument Sampler => tromboneAudio != null ? tromboneAudio.sampler : null;

    #region Input/Output interfaces
    public bool? Grab { get => grab; set { if (value != null) grab = value.Value; } }
    public bool? Blow { get => blow; set { if (value != null) blow = value.Value; } }
    public float? SlideTone { get => slideTone; set { if (value != null) slideTone = value.Value; } }
    public float? PressureLevel { get => pressureLevel; set { if (value != null) pressureLevel = value.Value; } }
    public float? PressureTone { get => GetTone(PressureIndex, 0f); set { if (value != null) TryGetPressureLevel(value.Value, out pressureLevel); } }
    #endregion


    private void OnEnable()
    {
        if (showDebug) Debug.Log("Enabling " + name);
        // Enable trombone components
        tromboneDisplay.enabled = true;
        tromboneAuto.enabled = true;
        tromboneAudio.enabled = true;
    }

    private void OnDisable()
    {
        if (showDebug) Debug.Log("Disabling " + name);
        ResetTrombone();
        // Disable trombone components
        tromboneDisplay.enabled = false;
        tromboneAuto.enabled = false;
        tromboneAudio.enabled = false;
    }

    public void ClearInputs()
    {
        grab = false;
        blow = false;
        slideTone = 0f;
        pressureLevel = 0f;
    }

    public void ResetTrombone()
    {
        ClearInputs();
        tromboneDisplay.ResetDisplay();
    }

    public float GetTone(float pressureLevel, float slideTone) => GetTone(RoundToPressureIndex(pressureLevel), slideTone);

    public float GetTone(int pressureIndex, float slideTone)
    {
        float tone = baseTone;
        int stepCount = pressureStepTones == null ? 0 : pressureStepTones.Length;
        if (stepCount > 0)
        {
            if (pressureIndex < 0) tone += pressureStepTones[0] * pressureIndex;
            else
            {
                for (int p = 0; p < pressureIndex; p++)
                {
                    if (p < stepCount) tone += pressureStepTones[p];
                    else tone += pressureStepTones[stepCount - 1];
                }
            }
        }
        return tone - slideTone;
    }

    public int RoundToPressureIndex(float pressureLevel) => Mathf.RoundToInt(pressureLevel);

    public bool TryGetPressureIndex(float tone, out int pressure)
    {
        pressure = 0;
        float pressureTone = baseTone;
        int stepCount = pressureStepTones == null ? 0 : pressureStepTones.Length;
        // Look for positive pressure index
        if (pressureTone < tone)
        {
            while (pressureTone < tone)
            {
                if (pressure < stepCount)
                {
                    pressureTone += pressureStepTones[pressure];
                    pressure++;
                }
                else
                {
                    float highStep = pressureStepTones[stepCount - 1];
                    if (highStep > 0)
                    {
                        int missingSteps = Mathf.CeilToInt((tone - pressureTone) / highStep);
                        pressureTone += highStep * missingSteps;
                        pressure += missingSteps;
                    }
                }
            }
        }
        // Look for negative pressure index
        else
        {
            float lowStep = stepCount > 0 ? pressureStepTones[0] : 0f;
            if (lowStep > 0)
            {
                int missingSteps = Mathf.CeilToInt((tone - pressureTone) / lowStep);
                pressureTone -= lowStep * missingSteps;
                pressure -= missingSteps;
            }
        }
        // Check if tone is within slide reach
        return pressureTone - tone <= slideTones;
    }
    public bool TryGetPressureLevel(float tone, out float pressure)
    {
        bool success = TryGetPressureIndex(tone, out int pressureIndex);
        pressure = pressureIndex;
        return success;
    }

    public bool TryGetSlideTone(float tone, int pressureIndex, out float slide) => TryGetSlideTone(tone, GetTone(pressureIndex, 0f), out slide);

    public bool TryGetSlideTone(float tone, float pressureTone, out float slide)
    {
        slide = pressureTone - tone;
        return slide >= 0f && slide <= slideTones;
    }

    public void Freeze()
    {
        if (tromboneAuto != null) tromboneAuto.enabled = false;
        if (tromboneDisplay != null) tromboneDisplay.enabled = false;
    }

    public void Unfreeze()
    {
        if (tromboneAuto != null) tromboneAuto.enabled = true;
        if (tromboneDisplay != null) tromboneDisplay.enabled = true;
    }
}