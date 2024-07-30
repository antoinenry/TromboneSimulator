using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Tempo")]
public class TempoModifier : TromboneBuildModifier
{
    public float tempoMultiplier = 1f;
    public bool multiplyScoreByTempo = true;
    public override string StatDescription => "Tempo x" + tempoMultiplier + "\n" + base.StatDescription;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.musicPlayer != null) build.musicPlayer.tempoModifier *= tempoMultiplier;
        if (multiplyScoreByTempo && build?.performanceJudge != null) build.performanceJudge.scoringRate *= tempoMultiplier;
    }

    public override bool CanStackWith(TromboneBuildModifier other)
    {
        if (tempoMultiplier == 0f) return false;
        if (base.CanStackWith(other)) return true;
        if (tempoMultiplier >= 1f) return (other as TempoModifier).tempoMultiplier >= 1f;
        else return (other as TempoModifier).tempoMultiplier < 1f;
    }
}