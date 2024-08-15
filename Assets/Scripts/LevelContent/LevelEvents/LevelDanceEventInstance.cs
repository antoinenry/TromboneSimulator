using UnityEngine;

public class LevelDanceEventInstance : LevelEventInstance<LevelDanceEventInfo>
{
    [Header("Configuration")]
    [SerializeField] private LevelDanceEventInfo eventInfo;

    public override LevelDanceEventInfo EventInfo
    {
        get => eventInfo;
        set => eventInfo = value;
    }

    private DanceForPoints danceForPoints;

    protected override void Awake()
    {
        base.Awake();
        danceForPoints = GetComponent<DanceForPoints>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (danceForPoints) danceForPoints.enabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (danceForPoints) danceForPoints.enabled = false;
    }

    public override void StartEvent()
    {
        base.StartEvent();
        if (danceForPoints != null)
        {
            danceForPoints.basePointsPerBeat = eventInfo.pointsPerBeat;
            danceForPoints.MaxDanceLevel = eventInfo.danceLevel;
            danceForPoints.onDanceCount.AddListener(OnDanceCount);
        }
    }

    private void OnDanceCount(int count, int maxCount)
    {
        float completion = (float)count / maxCount;
        onCompletion.Invoke(this, completion);
    }
}