using System;
using UnityEngine;

public class LevelSoloEventInstance : LevelEventInstance<LevelSoloEventInfo>
{
    [Header("Configuration")]
    [SerializeField] private LevelSoloEventInfo eventInfo;

    public override LevelSoloEventInfo EventInfo
    {
        get => eventInfo;
        set => eventInfo = value;
    }

    private PlayNotesForPoints notesForPoints;

    protected override void Awake()
    {
        base.Awake();
        notesForPoints = GetComponent<PlayNotesForPoints>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (notesForPoints) notesForPoints.enabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (notesForPoints) notesForPoints.enabled = false;
    }

    public override void StartEvent()
    {
        base.StartEvent();
        if (notesForPoints != null)
        {
            notesForPoints.noteCount = GetSoloLength();
            notesForPoints.onNoteCount.AddListener(OnNoteCount);
        }
    }

    public override void EndEvent()
    {
        base.EndEvent();
        notesForPoints?.onNoteCount?.RemoveListener(OnNoteCount);
    }

    private int GetSoloLength()
    {
        NoteInstance[] loadedNotes = FindObjectOfType<MusicPlayer>(true)?.LoadedNotes;
        if (loadedNotes == null) return 0;
        FloatSegment soloTime = new FloatSegment(eventInfo.StartTime, eventInfo.EndTime);
        NoteInstance[] soloNotes = Array.FindAll(loadedNotes, n => n != null && soloTime.Contains(n.StartTime));
        return soloNotes != null ? soloNotes.Length : 0;
    }

    private void OnNoteCount(int playedNotes, int missedNotes, int totalNotes)
    {
        float completion = (float)playedNotes / totalNotes;
        onCompletion.Invoke(this, completion);
    }
}