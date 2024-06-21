using UnityEngine;

public abstract class TromboneBuildModifier : ScriptableObject
{
    public Sprite icon;
    public float scoreMultiplier = 1f;

    public abstract bool CanStack { get; }
    public abstract void ApplyTo(TromboneCustomizer build);
}

