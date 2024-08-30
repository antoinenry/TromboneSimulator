using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TextSheet", menuName = "Trombone Hero/Level Events/Text Sheet")]
public class LevelTextSheet : LevelEventSheet<LevelTextEventInfo>
{
    public LevelTextEventInfo[] textInfos;

    public override Type EventInstanceType => typeof(LevelTextEventInstance);
    public override int EventCount => textInfos != null ? textInfos.Length : 0;

    public override LevelTextEventInfo[] GetEvents()
    {
        int count = EventCount;
        LevelTextEventInfo[] get = new LevelTextEventInfo[count];
        Array.Copy(textInfos, get, count);
        return get;
    }

    public override LevelTextEventInfo GetEvent(int eventIndex) => textInfos[eventIndex];

    public override void SetEvent(int eventIndex, LevelTextEventInfo eventInfo) => textInfos[eventIndex] = eventInfo;
}
