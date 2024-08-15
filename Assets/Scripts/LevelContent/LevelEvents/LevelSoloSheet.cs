using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoloSheet", menuName = "Trombone Hero/Level Events/Solo Sheet")]
public class LevelSoloSheet : LevelEventSheet<LevelSoloEventInfo>
{
    public LevelSoloEventInfo[] soloInfos;

    public override Type EventInstanceType => typeof(LevelSoloEventInstance);
    public override int EventCount => soloInfos != null ? soloInfos.Length : 0;

    public override LevelSoloEventInfo[] GetEvents()
    {
        int count = EventCount;
        LevelSoloEventInfo[] get = new LevelSoloEventInfo[count];
        Array.Copy(soloInfos, get, count);
        return get;
    }

    public override LevelSoloEventInfo GetEvent(int eventIndex) => soloInfos[eventIndex];

    public override void SetEvent(int eventIndex, LevelSoloEventInfo eventInfo) => soloInfos[eventIndex] = eventInfo;
}
