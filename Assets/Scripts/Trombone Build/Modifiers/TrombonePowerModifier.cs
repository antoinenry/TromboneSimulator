using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Power")]
public class TrombonePowerModifier : TromboneBuildModifier
{
    public GameObject powerPrefab;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.trombonePower != null)
        {
            build.trombonePower.powerPrefab = powerPrefab;
        }
    }
}