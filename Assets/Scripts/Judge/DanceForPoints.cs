using UnityEngine;
using UnityEngine.Events;
using static CounterUtility;

public class DanceForPoints : MonoBehaviour
{
    [Header("Components")]
    public DanceForPointsGUI GUI;
    public Counter danceCounter;
    [Header("Configuration")]
    public int basePointsPerBeat = 10;
    public float multiplierPerBeat = 1f;

    public UnityEvent<int,int> onDanceCount;

    private DanceDetector dance;
    private PerformanceJudge judge;

    public int PointsPerBeat => multiplierPerBeat != 0f ? (int)(basePointsPerBeat * multiplierPerBeat * DanceLevel) : basePointsPerBeat * DanceLevel;
    public int DanceLevel => danceCounter != null ? danceCounter.Value : 0;
    public int MaxDanceLevel
    {
        set
        {
            if (danceCounter != null) danceCounter.maxValue = value;
            if (GUI) GUI.SetGauge(maxValue: value);
        }

        get => danceCounter != null ? danceCounter.maxValue : 0;
    }

    private void Awake()
    {
        dance = FindObjectOfType<DanceDetector>(true);
        judge = FindObjectOfType<PerformanceJudge>(true);
    }

    private void OnEnable()
    {
        if (GUI != null && danceCounter != null)
        {
            danceCounter.Value = 0;
            GUI.SetGauge(0, danceCounter.maxValue);
            GUI.SetPoints(PointsPerBeat);
        }
        AddDanceListeners();
    }

    private void OnDisable()
    {
        RemoveDanceListeners();
    }

    private void AddDanceListeners()
    {
        dance?.onDanceBeat?.AddListener(OnDanceBeat);
        dance?.onMissBeat?.AddListener(OnMissBeat);
    }

    private void RemoveDanceListeners()
    {
        dance?.onDanceBeat?.RemoveListener(OnDanceBeat);
        dance?.onMissBeat?.RemoveListener(OnMissBeat);
    }

    private void OnDanceBeat()
    {
        if (danceCounter != null)
        {
            danceCounter.Increment();
            onDanceCount.Invoke(DanceLevel, MaxDanceLevel);
        }
        if (GUI)
        {
            GUI.SetPoints(PointsPerBeat);
            GUI.SetGauge(DanceLevel);
        }
        judge?.AddScore(PointsPerBeat);
    }

    private void OnMissBeat()
    {
        if (danceCounter != null)
        {
            danceCounter.Decrement();
            onDanceCount.Invoke(DanceLevel, MaxDanceLevel);
        }
        GUI?.SetGauge(DanceLevel);
    }
}
