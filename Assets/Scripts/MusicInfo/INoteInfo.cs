using System.Collections.Generic;

public interface INoteInfo : ITimingInfo
{
    public float Tone { get; set; }
    public float Velocity { get; set; }
}

public class NoteInfoComparer : IComparer<INoteInfo>
{
    public bool compareStartTime;
    public bool compareTone;
    public bool invertTime;
    public bool invertTone;

    public int Compare(INoteInfo x, INoteInfo y)
    {
        if (compareStartTime)
        {
            if (x.StartTime > y.StartTime) return invertTime ? -1 : 1;
            if (x.StartTime < y.StartTime) return invertTime ? 1 : -1;
        }
        if (compareTone)
        {
            if (x.Tone > y.Tone) return invertTone ? -1 : 1;
            if (x.Tone < y.Tone) return invertTone ? 1 : -1;
        }
        return 0;
    }
}