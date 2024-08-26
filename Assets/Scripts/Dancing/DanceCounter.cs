using UnityEngine;
using UnityEngine.Events;
using static CounterUtility;

public class DanceCounter : MonoBehaviour
{
    public Counter counter;

    public UnityEvent<int,int> onIncrease;
    public UnityEvent<int, int> onDecrease;

    private DanceDetector dance;

    public int DanceCount => counter != null ? counter.Value : 0;

    public int MaxDanceCount
    {
        set
        {
            if (counter != null) counter.maxValue = value;
        }

        get => counter != null ? counter.maxValue : 0;
    }

    private void Awake()
    {
        dance = FindObjectOfType<DanceDetector>(true); 
    }

    private void OnEnable()
    {
        if (counter != null) counter.Value = 0;
        AddDanceListeners();
    }

    private void OnDisable()
    {
        RemoveDanceListeners();
    }

    private void AddDanceListeners()
    {
        if (dance == null) return;
        dance.onDanceBeat?.AddListener(OnDanceBeat);
        dance.onMissBeat?.AddListener(OnMissBeat);
    }

    private void RemoveDanceListeners()
    {
        if (dance == null) return;
        dance.onDanceBeat?.RemoveListener(OnDanceBeat);
        dance.onMissBeat?.RemoveListener(OnMissBeat);
    }

    private void OnDanceBeat()
    {
        if (counter != null) counter.Increment();
        onIncrease.Invoke(DanceCount, MaxDanceCount);
    }

    private void OnMissBeat()
    {
        if (counter != null) counter.Decrement();
        onDecrease.Invoke(DanceCount, MaxDanceCount);
    }
}
