using UnityEngine;

public class DanceForCombo : DancePower
{
    public float comboPerBeat = 1f;

    private PerformanceJudge perfJudge;
    private float comboCounter;

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
            int previousCountFloored = Mathf.FloorToInt(comboCounter);
            comboCounter += comboPerBeat;
            int addToCombo = Mathf.FloorToInt(comboCounter) - previousCountFloored;
            if (addToCombo > 0)
            {
                perfJudge.SetCombo(perfJudge.combo + addToCombo);
                particleEffect.Play();
            }
        }
    }

    protected override void OnMissBeat()
    {
        base.OnMissBeat();
        comboCounter = 0f;
    }
}
