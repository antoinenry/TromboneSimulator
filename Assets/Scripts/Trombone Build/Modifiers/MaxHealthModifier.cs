using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Max Health")]
public class MaxHealthModifier : TromboneBuildModifier
{
    public float additionalMaxHealth = 0f;

    public override string StatDescription
    {
        get
        {
            string sign = additionalMaxHealth > 0f ? "+" : "";
            return sign + additionalMaxHealth + " hp\n" + base.StatDescription;
        }
    }

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.performanceJudge != null) build.performanceJudge.maxHealth += additionalMaxHealth;
    }

    public override bool CanStackWith(TromboneBuildModifier other)
    {
        return true;
    }
}
