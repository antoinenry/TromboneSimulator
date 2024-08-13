using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DanceSheet", menuName = "Trombone Hero/Level Events/Dance Sheet")]
public class LevelDanceSheet : LevelEventSheet<LevelDanceEventInfo>
{
    public LevelDanceEventInfo[] danceInfos;

    public override Type EventInstanceType => typeof(LevelDanceEventInstance);

    public override int EventCount => danceInfos != null ? danceInfos.Length : 0;

    public override ITimingInfo[] GetEvents()
    {
        int count = EventCount;
        ITimingInfo[] events = new ITimingInfo[count];
        Array.Copy(danceInfos, events, count);
        return events;
    }
}
