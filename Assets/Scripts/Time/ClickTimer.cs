using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class ClickTimer
{
    public Metronome metronome;
    public float step = 4f;
    public float stepsPerBeat = 1f;

    public UnityEvent<int> onStep;

    public int StepInt => stepsPerBeat >= 0f ? Mathf.FloorToInt(step) : Mathf.CeilToInt(step);

    public ClickTimer(Metronome m)
    {
        metronome = m;
        onStep = new UnityEvent<int>();
    }

    public void SetMaxSpeed(float maxStepsPerSecond)
    {
        //stepsPerBeat = maxStepsPerSecond > 0f ? 1f : -1f;
        //float beatsPerSeconds = metronome.BPM / 60f;
        //while (Mathf.Abs(beatsPerSeconds) > Mathf.Abs(maxStepsPerSecond))
        //{
        //    beatsPerSeconds /= 2f;
        //    stepsPerBeat /= 2f;
        //}
    }

    public void StartOnNextBeat()
    {
        //metronome.onBeat.AddListener(Start);
    }

    public void StartOnNextBar()
    {
        //metronome.onBar.AddListener(Start);
    }

    public void Start()
    {
        Stop();
        //metronome.onProgress.AddListener(OnMetronomeProgress);
    }

    public void Stop()
    {
        //metronome.onBeat.RemoveListener(Start);
        //metronome.onBar.RemoveListener(Start);
        //metronome.onProgress.RemoveListener(OnMetronomeProgress);
    }

    private void OnMetronomeProgress(float deltaBar, float deltaClick, float deltaBeat)
    {
        float delta = deltaClick;
        if (delta < 0f) delta += 1f;
        int lastStepInt = StepInt;
        step += stepsPerBeat * delta;
        int newStepInt = StepInt;
        if (newStepInt != lastStepInt) onStep.Invoke(newStepInt);
    }
}