using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Trombone Color")]
public class TromboneColorModifier : TromboneBuildModifier
{
    public Color color = Color.white;

    public override bool ReplaceOnly => true;
    public override Color IconColor => color;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.tromboneDisplay != null)
        {
            build.tromboneDisplay.color = color;
        }
    }
}