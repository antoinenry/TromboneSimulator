using System;

public abstract class UnlockableGameContent
{    
    public bool unlocked;
    public int unlockingTier;
}

[Serializable]
public class UnlockableLevel : UnlockableGameContent
{
    public Level levelAsset;
}

[Serializable]
public class UnlockableModifier : UnlockableGameContent
{
    public TromboneBuildModifier modifierAsset;
}