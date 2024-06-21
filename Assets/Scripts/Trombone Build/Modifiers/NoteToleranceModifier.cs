using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Tolerance")]
public class NoteToleranceModifier : TromboneBuildModifier
{
    public float rythmToleranceMultiplier = 1f;
    public float toneToleranceMultiplier = 1f;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.noteCatcher != null)
        {
            build.noteCatcher.advanceTolerance *= rythmToleranceMultiplier;
            build.noteCatcher.delayTolerance *= rythmToleranceMultiplier;
            build.noteCatcher.toneTolerance *= toneToleranceMultiplier;
        }
    }

    public override bool CanStackWith(TromboneBuildModifier other)
    {
        return true;
    }
}