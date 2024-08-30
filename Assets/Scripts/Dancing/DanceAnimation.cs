using UnityEngine;

public class DanceAnimation : MonoBehaviour
{
    public enum DanceRythm { None, OnBeat, OnBar }

    public Vector2 danceAmplitude;
    public DanceRythm rythm = DanceRythm.OnBeat;
    public bool flipX = false;
    public bool flipY = false;
    public AnimationCurve customCurve;

    private Metronome metronome;
    private Vector2 idlePosition;
    private Vector2 idleScale;

    private void Awake()
    {
        metronome = FindObjectOfType<Metronome>(true);
        idlePosition = transform.localPosition;
        idleScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.localPosition = idlePosition;
        transform.localScale = idleScale;
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
        // Position
        Vector2 dancePosition = Vector2.zero;
        if (customCurve.length <= 1)
        {
            dancePosition.x = Mathf.Round(danceAmplitude.x * Mathf.Abs(Mathf.Sin(time * Mathf.PI)));
            dancePosition.y = Mathf.Round(danceAmplitude.y * Mathf.Abs(Mathf.Sin(time * Mathf.PI)));
        }
        else
        {
            float time01 = Mathf.Repeat(time, 1f);
            dancePosition.x = Mathf.Round(danceAmplitude.x * customCurve.Evaluate(time01));
            dancePosition.y = Mathf.Round(danceAmplitude.y * customCurve.Evaluate(time01));
        }
        transform.localPosition = idlePosition + dancePosition;
        // Flip
        Vector2 danceScale = Vector2.one;
        if (flipX) danceScale.x = time < .5f ? 1f : -1f;
        if (flipY) danceScale.y = time < .5f ? 1f : -1f;
        transform.localScale = Vector2.Scale(idleScale, danceScale);
    }
}
