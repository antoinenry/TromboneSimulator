using UnityEngine;
using UnityEngine.Events;

public class DanceDetector : MonoBehaviour
{
    public Transform dancer;
    [Range(0f, 1f)] public float minimumAccuracy = .9f;
    [Header("Events")]
    public UnityEvent onDanceBeat;
    public UnityEvent onMissBeat;

    private float dancerY;
    private float lowTime;
    private bool dancingDown;
    private float accuracy;

    public Metronome Metronome { get; private set; }
    public int DanceCounter { get; private set; }

    private void Awake()
    {
        Metronome = FindObjectOfType<Metronome>(true);
    }

    private void OnEnable()
    {
        Metronome.onTimeChange.AddListener(OnMetronomeProgress);
        Metronome.onBeatChange.AddListener(OnMetronomeBeat);
        DanceCounter = 0;
    }

    private void OnDisable()
    {
        Metronome.onTimeChange.RemoveListener(OnMetronomeProgress);
        Metronome.onBeatChange.RemoveListener(OnMetronomeBeat);
        DanceCounter = 0;
    }


    private void OnMetronomeProgress(float fromTime, float toTime)
    {
        float y = dancer.position.y;
        if (y < dancerY) dancingDown = true;
        else if (y > dancerY && dancingDown == true)
        {
            float time = Time.time;
            float dancePeriod = Mathf.Abs(time - lowTime);
            float error = Mathf.Abs(dancePeriod / Metronome.CurrentBeat.duration - 1f);
            accuracy = 1f - Mathf.Clamp01(error);
            lowTime = Time.time;
            dancingDown = false;
        }
        dancerY = y;
    }

    private void OnMetronomeBeat(int fromBeat, int toBeat)
    {
        if (accuracy >= minimumAccuracy)
        {
            onDanceBeat.Invoke();
            DanceCounter++;
        }
        else
        {
            onMissBeat.Invoke();
            DanceCounter = 0;
        }
        accuracy = 0f;
    }
}
