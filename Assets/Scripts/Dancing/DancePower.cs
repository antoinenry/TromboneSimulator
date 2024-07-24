using UnityEngine;

public class DancePower : MonoBehaviour
{
    public ParticleSystem particleEffect;
    public int beatsToStart = 0;
    public Color frameTint = Color.yellow;

    protected TromboneDisplay trombone;
    protected DanceDetector dance;
    protected JudgeGUI perfGUI;
    protected TintFlash frame;

    protected virtual void Awake()
    {
        trombone = FindObjectOfType<TromboneDisplay>(true);
        dance = FindObjectOfType<DanceDetector>(true);
        dance.dancer = trombone.body.transform;
        perfGUI = FindObjectOfType<JudgeGUI>(true);
        frame = perfGUI.frame;
    }

    protected virtual void OnEnable()
    {
        dance.onDanceBeat.AddListener(OnDanceBeat);
        dance.onMissBeat.AddListener(OnMissBeat);
        particleEffect.Stop();
    }

    protected virtual void OnDisable()
    {
        dance.onDanceBeat.RemoveListener(OnDanceBeat);
        dance.onMissBeat.RemoveListener(OnMissBeat);
        particleEffect.Stop();
    }

    protected virtual void Update()
    {
        particleEffect.transform.position = trombone.GrabPosition;
    }

    protected virtual void OnDanceBeat()
    {
        if (dance.DanceCounter > beatsToStart)
        {
            frame.Tint(frameTint);
            particleEffect.Play();
        }
    }

    protected virtual void OnMissBeat()
    {
        particleEffect.Stop();
    }
}