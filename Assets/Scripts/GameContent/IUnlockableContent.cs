using System.Collections;
using System.Collections.Generic;

public interface IUnlockableContent
{
    public bool AutoUnlock { get; }
    public int UnlockTier { get; }
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