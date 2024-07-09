using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Tolerance")]
public class NoteToleranceModifier : TromboneBuildModifier
{
    public float rythmToleranceMultiplier = 1f;
    public float toneToleranceMultiplier = 1f;

    public override string StatDescription
    {
        get
        {
            string d = "";
            if (rythmToleranceMultiplier != 1f)
                d += "Tolerance rythme x" + rythmToleranceMultiplier + "\n";
            if (toneToleranceMultiplier != 1f)
                d += "Tolerance justesse x" + toneToleranceMultiplier + "\n";
            d += base.StatDescription;
            return d;
        }
    }

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