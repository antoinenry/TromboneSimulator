using UnityEngine;

[ExecuteAlways]
public class TromboneCore : MonoBehaviour,
    ITromboneGrabInput, ITromboneBlowInput, ITromboneSlideToneInput, ITrombonePressureLevelInput, ITrombonePressureToneInput,
    ITromboneGrabOutput, ITromboneBlowOutput, ITromboneSlideToneOutput, ITrombonePressureLevelOutput, ITrombonePressureToneOutput
{
    public bool showDebug = false;
    [Header("Components")]
    public TromboneDisplay tromboneDisplay;
    public TromboneAudio tromboneAudio;
    public TromboneAuto tromboneAuto;
    public TrombonePowerSlot trombonePower;
    public TromboneBuildStack tromboneBuild;
    [Header("Controls")]
    public bool grab;
    public bool blow;
    public float slideTone;
    public float pressureLevel;
    [Header("Tone")]
    [Tone] public float baseTone;
    public float[] pressureToneSteps;
    public float slideToneLength = 6f;

    public int PressureIndex => RoundToPressureIndex(pressureLevel);
    public float Tone => GetTone(PressureIndex, slideTone);
    public float[] PressureTones
    {
        get
        {
            int stepCount = pressureToneSteps == null ? 0 : pressureToneSteps.Length;
            float[] tones = new float[stepCount + 1];
            tones[0] = baseTone;
            for (int p = 0; p < stepCount; p++) tones[p + 1] = tones[p] + pressureToneSteps[p];
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
        if (tromboneDisplay) tromboneDisplay.enabled = true;
        if (tromboneAuto) tromboneAuto.enabled = true;
        if (tromboneAudio) tromboneAudio.enabled = true;
        if (tromboneBuild) tromboneBuild.enabled = true;
    }

    private void OnDisable()
    {
        if (showDebug) Debug.Log("Disabling " + name);
        ResetTrombone();
        // Disable trombone components
        if (tromboneDisplay) tromboneDisplay.enabled = false;
        if (tromboneAuto) tromboneAuto.enabled = false;
        if (tromboneAudio) tromboneAudio.enabled = false;
        if (tromboneBuild) tromboneBuild.enabled = false;
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
        tromboneBuild?.ApplyStack();
        tromboneDisplay?.ResetDisplay();
    }

    public float GetTone(float pressureLevel, float slideTone) => GetTone(RoundToPressureIndex(pressureLevel), slideTone);

    public float GetTone(int pressureIndex, float slideTone)
    {
        float tone = baseTone;
        int stepCount = pressureToneSteps == null ? 0 : pressureToneSteps.Length;
        if (stepCount > 0)
        {
            if (pressureIndex < 0) tone += pressureToneSteps[0] * pressureIndex;
            else
            {
                for (int p = 0; p < pressureIndex; p++)
                {
                    if (p < stepCount) tone += pressureToneSteps[p];
                    else tone += pressureToneSteps[stepCount - 1];
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
        int stepCount = pressureToneSteps == null ? 0 : pressureToneSteps.Length;
        // Look for positive pressure index
        if (pressureTone < tone)
        {
            while (pressureTone < tone)
            {
                if (pressure < stepCount)
                {
                    pressureTone += pressureToneSteps[pressure];
                    pressure++;
                }
                else
                {
                    float highStep = pressureToneSteps[stepCount - 1];
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
            float lowStep = stepCount > 0 ? pressureToneSteps[0] : 0f;
            if (lowStep > 0)
            {
                int missingSteps = Mathf.CeilToInt((tone - pressureTone) / lowStep);
                pressureTone -= lowStep * missingSteps;
                pressure -= missingSteps;
            }
        }
        // Check if tone is within slide reach
        return pressureTone - tone <= slideToneLength;
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
        return slide >= 0f && slide <= slideToneLength;
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