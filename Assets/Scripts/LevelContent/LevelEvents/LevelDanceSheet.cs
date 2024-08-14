using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DanceSheet", menuName = "Trombone Hero/Level Events/Dance Sheet")]
public class LevelDanceSheet : LevelEventSheet<LevelDanceEventInfo>
{
    public LevelDanceEventInfo[] danceInfos;

    public override Type EventInstanceType => typeof(LevelDanceEventInstance);
    public override int EventCount => danceInfos != null ? danceInfos.Length : 0;

    public override LevelDanceEventInfo[] GetEvents()
    {
        int count = EventCount;
        LevelDanceEventInfo[] get = new LevelDanceEventInfo[count];
        Array.Copy(danceInfos, get, count);
        return get;
    }

    public override LevelDanceEventInfo GetEvent(int eventIndex) => danceInfos[eventIndex];

    public override void SetEvent(int eventIndex, LevelDanceEventInfo eventInfo) => danceInfos[eventIndex] = eventInfo;
}
