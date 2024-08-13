using UnityEngine;

public class PowerDanceForPoints : DancePower
{
    public float pointsPerBeat = 100f;

    private PerformanceJudge perfJudge;

    protected override void Awake()
    {
        base.Awake();
        perfJudge = FindObjectOfType<PerformanceJudge>(true);
    }

    protected override void OnDanceBeat()
    {
        if (dance.DanceCounter > beatsToStart)
        {
            perfGUI.frame.Tint(frameTint);
            perfJudge.AddScore(pointsPerBeat);
            particleEffect.Play();
        }
    }
}
