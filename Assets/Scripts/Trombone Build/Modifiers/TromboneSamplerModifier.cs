using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Trombone Sampler")]
public class TromboneSamplerModifier : TromboneBuildModifier
{
    public SamplerInstrument samplerInstrument;

    public override bool ReplaceOnly => true;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.tromboneAudio != null)
        {
            build.tromboneAudio.sampler = samplerInstrument;
        }
    }
}