using UnityEngine;
using UnityEngine.Events;

public abstract class LevelEventInstance : MonoBehaviour, ITimingInfo
{
    [Header("Components")]
    public GameObject GUI;
    [Header("Events")]
    public UnityEvent<LevelEventInstance, float> onCompletion;

    private GameObjectDispatch GUIDispatch;

    protected virtual void Awake()
    {
        GUIDispatch = new GameObjectDispatch();
        LevelGUI levelGUI = FindObjectOfType<LevelGUI>(true);
        if (GUI && levelGUI) GUIDispatch.Dispatch(GUI.transform, levelGUI.eventPanel);
    }

    protected virtual void OnEnable()
    {
        if (GUI) GUI.SetActive(true);
    }

    protected virtual void OnDisable()
    {
        if (GUI) GUI.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        GUIDispatch?.DestroyDispatched();
    }

    public abstract ITimingInfo TimingInfo { get; set; }
    public float StartTime { get => TimingInfo.StartTime; set => TimingInfo.StartTime = value; }
    public float Duration { get => TimingInfo.Duration; set => TimingInfo.Duration = value; }
    public float EndTime { get => StartTime + Duration; set => Duration = Mathf.Max(0f, value - StartTime); }

    public abstract void SetEventInfo(ITimingInfo info);
    public virtual void StartEvent() => enabled = true;
    public virtual void EndEvent() => enabled = false;
}

public abstract class LevelEventInstance<T> : LevelEventInstance where T : ITimingInfo
{
    public abstract T EventInfo { get; set; }

    public override ITimingInfo TimingInfo
    {
        get => EventInfo;
        set
        {
            EventInfo.StartTime = value.StartTime;
            EventInfo.Duration = value.Duration;
        }
    }

    public override void SetEventInfo(ITimingInfo info)
    {
        if (info is T) EventInfo = (T)info;
        else TimingInfo = info;
    }
}