using UnityEngine;

public class LevelDanceEventInstance : LevelEventInstance<LevelDanceEventInfo>
{
    [Header("Components")]
    public GameObject GUI;
    [Header("Configuration")]
    public LevelDanceEventInfo eventInfo;

    public override ITimingInfo TimingInfo
    {
        get => eventInfo;
        set
        {
            eventInfo.StartTime = value.StartTime;
            eventInfo.Duration = value.Duration;
        }
    }

    public override LevelDanceEventInfo EventInfo
    {
        get => eventInfo;
        set => eventInfo = value;
    }

    private DanceForPoints danceForPoints;

    public void Awake()
    {
        danceForPoints = GetComponent<DanceForPoints>();
        MoveGUIToContainer();
    }

    private void OnEnable()
    {
        GUI?.SetActive(true);
        if (danceForPoints) danceForPoints.enabled = true;
    }

    private void OnDisable()
    {
        GUI?.SetActive(false);
        if (danceForPoints) danceForPoints.enabled = false;
    }

    private void MoveGUIToContainer()
    {
        if (GUI == null) return;
        RectTransform container = FindObjectOfType<LevelGUI>(true)?.eventDisplayContainer;
        if (container == null) return;
        GUI.transform.SetParent(container);
    }

    private void OnDestroy()
    {
        if (GUI != null) DestroyImmediate(GUI);
    }

    public override void StartEvent()
    {
        base.StartEvent();
        if (danceForPoints != null)
        {
            danceForPoints.basePointsPerBeat = eventInfo.pointsPerBeat;
            danceForPoints.MaxDanceLevel = eventInfo.danceLevel;
        }
    }
}