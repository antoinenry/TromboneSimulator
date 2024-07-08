using UnityEngine;

public abstract class TromboneBuildModifier : ScriptableObject, IUnlockableContent
{
    public string modName;
    public Sprite icon;
    public int unlockTier;
    public float scoreMultiplierBonus = 1f;
    public string description;

    public bool AutoUnlock => false;
    public int UnlockTier => unlockTier;
    public virtual float ScoreMultiplier => scoreMultiplierBonus;
    public virtual Color IconColor => Color.white;
    public virtual string StatDescription => ScoreMultiplier != 1f ? "Score x" + ScoreMultiplier : null;

    public virtual bool CanStackWith(TromboneBuildModifier other)
        => other == null || other.GetType() != GetType();

    public virtual void ApplyTo(TromboneBuild build)
    {
        if (build?.performanceJudge != null) build.performanceJudge.scoringRate *= ScoreMultiplier;
    }
}

