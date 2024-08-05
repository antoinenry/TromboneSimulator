using System.Collections.Generic;
using UnityEngine;

public interface IUnlockableContent
{
    public ScriptableObject ContentAsset { get; }
    public bool AutoUnlock { get; }
    public int UnlockTier { get; set; }
}

public class UnlockTierComparer : IComparer<IUnlockableContent>
{
    public int Compare(IUnlockableContent x, IUnlockableContent y)
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;
        if (x.UnlockTier == y.UnlockTier) return 0;
        return x.UnlockTier > y.UnlockTier ? 1 : -1;
    }
}