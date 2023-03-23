using UnityEngine;

public class DanceAnimation : MonoBehaviour
{
    public enum DanceRythm { None, OnBeat, OnBar }

    public Vector2 danceAmplitude;
    public DanceRythm rythm = DanceRythm.OnBeat;
    public AnimationCurve customCurve;

    private Metronome metronome;
    private Vector2 idlePosition;

    private void Awake()
    {
        metronome = FindObjectOfType<Metronome>(true);
        idlePosition = transform.localPosition;
    }

    private void Update()
    {
        switch (rythm)
        {
            case DanceRythm.OnBeat:
                Dance(metronome.CurrentBeatProgress);
                break;
            case DanceRythm.OnBar: Dance(metronome.CurrentBarProgress);
                break;
        }
    }

    private void Dance(float time)
    {
        Vector2 dance;
        if (customCurve.length <= 1)
        {
            dance.x = Mathf.Round(danceAmplitude.x * Mathf.Abs(Mathf.Sin(time * Mathf.PI)));
            dance.y = Mathf.Round(danceAmplitude.y * Mathf.Abs(Mathf.Sin(time * Mathf.PI)));
        }
        else
        {
            float time01 = Mathf.Repeat(time, 1f);
            dance.x = Mathf.Round(danceAmplitude.x * customCurve.Evaluate(time01));
            dance.y = Mathf.Round(danceAmplitude.y * customCurve.Evaluate(time01));
        }
        transform.localPosition = idlePosition + dance;
    }
}
