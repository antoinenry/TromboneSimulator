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
        locked = true;
    }

    public void SetLocked(bool value) => locked = value;
}