using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Auto Blow")]
public class TromboneAutoBlowModifier : TromboneBuildModifier
{
    public TromboneAutoSettings.ControlConditions blowControl;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.tromboneAuto != null)
        {
            TromboneAutoSettings settings = build.tromboneAuto.autoSettings;
            settings.blowControl = blowControl;
            build.tromboneAuto.autoSettings = settings;
        }
    }
}