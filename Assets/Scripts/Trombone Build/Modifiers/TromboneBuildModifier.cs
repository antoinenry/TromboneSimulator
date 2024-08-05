using UnityEngine;

public abstract class TromboneBuildModifier : ScriptableObject, IUnlockableContent
{
    public string modName;
    public Sprite icon;
    public Color uiColor = Color.white;
    public bool colorizeIcon;
    public int unlockTier;
    public float scoreMultiplierBonus = 1f;
    public string description;

    public ScriptableObject ContentAsset => this; 
    public bool AutoUnlock => false;
    public int UnlockTier
    {
        get => unlockTier;
        set => unlockTier = value;
    }
    public virtual bool ReplaceOnly => false;
    public virtual float ScoreMultiplier => scoreMultiplierBonus;
    public virtual Color IconColor => colorizeIcon ? uiColor : Color.white;
    public virtual string StatDescription => ScoreMultiplier != 1f ? "Score x" + ScoreMultiplier : null;

    public virtual bool CanStackWith(TromboneBuildModifier other)
        => other == null || other.GetType() != GetType();

    public virtual void ApplyTo(TromboneBuild build)
    {
        if (build?.performanceJudge != null) build.performanceJudge.scoringRate *= ScoreMultiplier;
    }
}

