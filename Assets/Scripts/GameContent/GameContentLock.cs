using System;
using UnityEngine;

[Serializable]
public struct GameContentLock
{
    public ScriptableObject contentAsset;
    public bool locked;

    static public int GetUnlockTier(ScriptableObject contentAsset)
        => (contentAsset != null && contentAsset is IUnlockableContent) ? (contentAsset as IUnlockableContent).UnlockTier : -1;

    static public void SetUnlockTier(ScriptableObject contentAsset, int value)
    {
        if (contentAsset == null || contentAsset is IUnlockableContent == false) return;
        (contentAsset as IUnlockableContent).UnlockTier = value;
    }

    public int UnlockTier
    {
        get => GetUnlockTier(contentAsset);
        set => SetUnlockTier(contentAsset, value);
    }

    public GameContentLock(ScriptableObject content)
    {
        contentAsset = content;
        if (GetUnlockTier(content) <= 0) locked = false;
        else locked = true;
    }

    public void SetLocked(bool value) => locked = value;
}