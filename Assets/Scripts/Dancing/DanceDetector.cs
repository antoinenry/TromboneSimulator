using UnityEngine;
using UnityEngine.Events;

public class DanceDetector : MonoBehaviour
{
    public Transform dancer;
    [Range(0f, 1f)] public float minimumAccuracy = .75f;
    public float minimumAmplitude = 1f;
    [Header("Events")]
    public UnityEvent onDanceBeat;
    public UnityEvent onMissBeat;

    private float dancerY;
    private float lowTime;
    private float highY;
    private bool dancingDown;

    public Metronome Metronome { get; private set; }
    public int DanceCounter { get; private set; }
    public float DanceAccuracy {  get; private set; }
    public float DanceAmplitude {  get; private set; }

    private void Awake()
    {
        Metronome = FindObjectOfType<Metronome>(true);
    }

    private void OnEnable()
    {
        Metronome.onTimeChange.AddListener(OnMetronomeProgress);
        Metronome.onBeatChange.AddListener(OnMetronomeBeat);
        DanceCounter = 0;
        DanceAccuracy = 0;
        DanceAmplitude = 0;
    }

    private void OnDisable()
    {
        Metronome.onTimeChange.RemoveListener(OnMetronomeProgress);
        Metronome.onBeatChange.RemoveListener(OnMetronomeBeat);
        DanceCounter = 0;
        DanceAccuracy = 0;
        DanceAmplitude = 0;
    }


    private void OnMetronomeProgress(float fromTime, float toTime)
    {
        float y = dancer.position.y;
        // Moving down
        if (y < dancerY)
        {
            // Changing direction
            if (dancingDown == false)
            {
                highY = y;
                dancingDown = true;
            }
        }
        // Moving up
        else if (y > dancerY && dancingDown == true)
        {
            // Changing direction
            if (dancingDown == true)
            {
                float time = Time.time;
                float dancePeriod = Mathf.Abs(time - lowTime);
                float error = Mathf.Abs(dancePeriod / Metronome.CurrentBeat.duration - 1f);
                DanceAccuracy = 1f - Mathf.Clamp01(error);
                DanceAmplitude = highY - y;
                lowTime = Time.time;
                dancingDown = false;
            }
        }
        dancerY = y;
    }

    private void OnMetronomeBeat(int fromBeat, int toBeat)
    {
        if (DanceAccuracy >= minimumAccuracy && DanceAmplitude >= minimumAmplitude)
        {
            onDanceBeat.Invoke();
            DanceCounter++;
        }
        else
        {
            onMissBeat.Invoke();
            DanceCounter = 0;
        }
        DanceAccuracy = 0f;
    }
}
