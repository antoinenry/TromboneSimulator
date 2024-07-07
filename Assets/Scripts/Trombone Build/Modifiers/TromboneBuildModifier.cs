using UnityEngine;

public abstract class TromboneBuildModifier : ScriptableObject, IUnlockableContent
{
    public string modName;
    public Sprite icon;
    public bool replaceOnly;
    public int unlockTier;
    public float scoreMultiplier = 1f;

    public bool AutoUnlock => false;
    public int UnlockTier => unlockTier;

    public virtual bool CanStackWith(TromboneBuildModifier other)
        => other == null || other.GetType() != GetType();

    public virtual void ApplyTo(TromboneBuild build)
    {
        if (build?.performanceJudge != null) build.performanceJudge.scoringRate *= scoreMultiplier;
    }
}

