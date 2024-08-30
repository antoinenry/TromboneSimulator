using UnityEngine;
using UnityEngine.Events;

public class LevelDanceEventInstance : LevelEventInstance<LevelDanceEventInfo>
{
    [Header("Configuration")]
    [SerializeField] private LevelDanceEventInfo eventInfo;
    [Header("Events")]
    public UnityEvent<int> onPoints;

    private PerformanceJudge perfJudge;

    public override LevelDanceEventInfo EventInfo
    {
        get => eventInfo;
        set => eventInfo = value;
    }

    private DanceCounter danceCounter;

    protected override void Awake()
    {
        base.Awake();
        danceCounter = GetComponent<DanceCounter>();
        perfJudge = FindObjectOfType<PerformanceJudge>(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (danceCounter) danceCounter.enabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (danceCounter) danceCounter.enabled = false;
    }

    public override void StartEvent()
    {
        base.StartEvent();
        if (danceCounter)
        {
            danceCounter.MaxDanceCount = eventInfo.danceLevel;
            danceCounter.onIncrease.AddListener(OnDanceCountUp);
            danceCounter.onDecrease.AddListener(OnDanceCountDown);
        }
    }

    public override void EndEvent()
    {
        base.EndEvent();
        if (danceCounter != null)
        {
            danceCounter.onIncrease.RemoveListener(OnDanceCountUp);
            danceCounter.onDecrease.RemoveListener(OnDanceCountDown);
        }
    }

    private void OnDanceCountUp(int danceCount, int maxCount)
    {
        int points = eventInfo.basePointsPerBeat + eventInfo.pointMultiplierPerBeat * danceCount;
        if (perfJudge) perfJudge.AddScore(points);
        onPoints.Invoke(points);
        OnDanceCount(danceCount, maxCount);
    }

    private void OnDanceCountDown(int danceCount, int maxCount)
    {
        OnDanceCount(danceCount, maxCount);
    }

    private void OnDanceCount(int count, int maxCount)
    {
        float completion = (float)count / maxCount;
        onCompletion.Invoke(this, completion);
    }
}