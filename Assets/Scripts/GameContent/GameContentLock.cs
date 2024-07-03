using System;
using UnityEngine;

[Serializable]
public struct GameContentLock
{
    public ScriptableObject contentAsset;
    public bool locked;

    public GameContentLock(ScriptableObject content)
    {
        contentAsset = content;
        if (contentAsset is IUnlockableContent && (contentAsset as IUnlockableContent).UnlockTier <= 0) locked = false;
        else locked = true;
    }

    public void SetLocked(bool value) => locked = value;
}