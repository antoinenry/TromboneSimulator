using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class ClickTimer
{
    public Metronome metronome;
    public int step = 4;
    public int beatsPerStep = 1;

    public UnityEvent<int> onStep;

    private int beatCounter;

    public ClickTimer(Metronome m)
    {
        metronome = m;
        onStep = new UnityEvent<int>();
    }

    public void SetStepDuration(int durationInBeats, float minimumDurationInSeconds)
    {
        beatsPerStep = durationInBeats;
        // Adjust step duration to match required minimum
        float beatDuration = metronome.CurrentBeat.duration;
        float stepDuration = durationInBeats * beatDuration;
        if (stepDuration != 0f)
        {
            while (Mathf.Abs(stepDuration) < Mathf.Abs(minimumDurationInSeconds))
            {
                beatsPerStep *= 2;
                stepDuration *= 2f;
            }
        }
        else
            Debug.LogWarning("Beat duration is zero.");
    }

    public void StartOnNextBeat()
    {
        metronome.onBeatChange.AddListener(StartOnBeatChange);
    }

    public void StartOnNextBar()
    {
        metronome.onBarChange.AddListener(StartOnBarChange);
    }

    public void Start()
    {
        Stop();
        beatCounter = 0;
        metronome.onBeatChange.AddListener(StepOnBeat);
    }

    public void StartOnBeatChange(int previousBeat, int nextBeat) => Start();
    public void StartOnBarChange(int previousBar, int nextBar) => Start();

    public void Stop()
    {
        metronome.onBeatChange.RemoveListener(StartOnBeatChange);
        metronome.onBarChange.RemoveListener(StartOnBarChange);
        metronome.onBeatChange.RemoveListener(StepOnBeat);
    }

    private void StepOnBeat(int previousBeat, int nextBeat)
    {
        if (++beatCounter > Mathf.Abs(beatsPerStep))
        {
            if (beatsPerStep >= 0) step++;
            else step--;
            onStep.Invoke(step);
            beatCounter = 0;
        }

    }
}