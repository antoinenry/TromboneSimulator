using UnityEngine;
using UnityEngine.Events;
using static CounterUtility;

public class DanceCounter : MonoBehaviour
{
    public Counter counter;

    public UnityEvent<int,int> onIncrease;
    public UnityEvent<int, int> onDecrease;

    public DanceDetector Dance { get; private set; }

    public int DanceCount
    {
        get => counter != null ? counter.Value : 0;
        set
        {
            if (counter != null) counter.Value = value;
        }
    }

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
        Dance = FindObjectOfType<DanceDetector>(true); 
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
        if (Dance == null) return;
        Dance.onDanceBeat?.AddListener(OnDanceBeat);
        Dance.onMissBeat?.AddListener(OnMissBeat);
    }

    private void RemoveDanceListeners()
    {
        if (Dance == null) return;
        Dance.onDanceBeat?.RemoveListener(OnDanceBeat);
        Dance.onMissBeat?.RemoveListener(OnMissBeat);
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
