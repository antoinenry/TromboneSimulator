using UnityEngine;
using UnityEngine.Events;

public class DanceDetector : MonoBehaviour
{
    public Transform dancer;
    public float periodAccuracy = 0f;
    [Range(0f, 1f)] public float minimumAccuracy = .9f;
    [Header("Events")]
    public UnityEvent onDanceBeat;
    public UnityEvent onMissBeat;

    private Metronome metronome;
    private float dancerY;
    private float lowTime;
    private bool dancingDown;

    private void Awake()
    {
        metronome = FindObjectOfType<Metronome>(true);
    }

    private void OnEnable()
    {
        metronome.onTimeChange.AddListener(OnMetronomeProgress);
        metronome.onBeatChange.AddListener(OnMetronomeBeat);
    }

    private void OnDisable()
    {
        metronome.onTimeChange.RemoveListener(OnMetronomeProgress);
        metronome.onBeatChange.RemoveListener(OnMetronomeBeat);
    }


    private void OnMetronomeProgress(float fromTime, float toTime)
    {
        if (dancer != null)
        {
            float y = dancer.position.y;
            if (y < dancerY) dancingDown = true;
            else if (y > dancerY && dancingDown == true)
            {
                float time = Time.time;
                float dancePeriod = Mathf.Abs(time - lowTime);
                float error = Mathf.Abs(dancePeriod / metronome.CurrentBeat.duration - 1f);
                periodAccuracy = 1f - Mathf.Clamp01(error);
                lowTime = Time.time;
                dancingDown = false;
            }
            dancerY = y;
        }
        else
        {
            dancerY = 0;
            periodAccuracy = 0f;
            lowTime = 0f;
            dancingDown = false;
        }
    }

    private void OnMetronomeBeat(int fromBeat, int toBeat)
    {
        if (periodAccuracy >= minimumAccuracy) onDanceBeat.Invoke();
        else onMissBeat.Invoke();
        periodAccuracy = 0f;
    }
}
