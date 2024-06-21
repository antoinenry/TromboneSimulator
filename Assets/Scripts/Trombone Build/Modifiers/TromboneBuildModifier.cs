using UnityEngine;

public abstract class TromboneBuildModifier : ScriptableObject
{
    public string modName;
    public Sprite icon;
    public float scoreMultiplier = 1f;

    public virtual bool CanStackWith(TromboneBuildModifier other)
        => other == null || other.GetType() != GetType();

    public virtual void ApplyTo(TromboneBuild build)
    {
        if (build?.performanceJudge != null) build.performanceJudge.scoringRate *= scoreMultiplier;
    }
}

