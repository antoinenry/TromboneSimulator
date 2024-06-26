using System;
using UnityEngine;

[Serializable]
public struct GameContentLock
{
    public ScriptableObject contentAsset;
    public bool unlocked;

    public GameContentLock(ScriptableObject content)
    {
        contentAsset = content;
        unlocked = false;
    }

    public void Unlock() => unlocked = true;
}