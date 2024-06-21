public class DanceForHealing : DancePower
{
    public float healPerBeat = .1f;

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
            if (perfJudge.health < perfJudge.maxHealth)
            {
                perfJudge.HealDamage(healPerBeat);
                particleEffect.Play();
            }
        }
    }
}
