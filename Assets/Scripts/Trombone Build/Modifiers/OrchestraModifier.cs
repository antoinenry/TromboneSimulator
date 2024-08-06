using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Orchestra")]
public class OrchestraModifier : TromboneBuildModifier
{
    public Orchestra orchestra;

    public override bool ReplaceOnly => true;

    public override string StatDescription => "Change le son de l'accompagnement";

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.audioTrackGenerator != null) build.audioTrackGenerator.orchestra = orchestra;
    }
}