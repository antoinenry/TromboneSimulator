using UnityEngine;

public class LevelTextEventInstance : LevelEventInstance<LevelTextEventInfo>
{
    [Header("Configuration")]
    [SerializeField] private LevelTextEventInfo eventInfo;

    private LevelGUI gui;

    public override LevelTextEventInfo EventInfo
    {
        get => eventInfo;
        set => eventInfo = value;
    }

    protected override void Awake()
    {
        base.Awake();
        gui = FindObjectOfType<LevelGUI>(true);
    }

    public override void StartEvent()
    {
        base.StartEvent();
        if (gui != null && eventInfo.text != null && eventInfo.text != string.Empty) gui.ShowMessage(eventInfo.text, eventInfo.danceAnimation);
    }

    public override void EndEvent()
    {
        base.EndEvent();
        if (gui != null) gui.FreeMessage(eventInfo.text);
    }
}