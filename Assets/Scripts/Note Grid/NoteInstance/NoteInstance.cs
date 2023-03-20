using UnityEngine;
using System;

[RequireComponent(typeof(NoteDisplay))]
public class NoteInstance : MonoBehaviour, INote
{
    [Flags] public enum CatchState { Nothing = 0, Mixed = 1, All = 3 }

    public NoteInfo noteInfo;
    public NotePerformance performance;
    public CatchState catchState;
    public NoteDisplay display;

    public float Tone { get => noteInfo.tone; set => noteInfo.tone = value; }
    public float Velocity { get => noteInfo.velocity; set => noteInfo.velocity = value; }
    public float StartTime { get => noteInfo.startTime; set => noteInfo.startTime = value; }
    public float Duration { get => noteInfo.duration; set => noteInfo.duration = value; }

    public Color DisplayColor => display != null ? display.baseColor : Color.magenta;
    public float DisplayLength { get; private set; }
    public float DisplayDistance { get; private set; }

    private void Awake()
    {
        display = GetComponent<NoteDisplay>();
    }

    public void Init(float time, float timeScale, Color color, float incomingTime, bool linkToPreviousNote = false, bool linkToNextNote = false)
    {
        // Init display
        display.SetSprites(linkToPreviousNote, linkToNextNote);
        DisplayLength = Duration * timeScale;
        display.SetLength(DisplayLength);
        DisplayDistance = (StartTime - time) * timeScale;
        display.SetDistance(DisplayDistance);
        display.baseColor = color;
        display.incomingDistance = incomingTime * timeScale;
        // Init catch state
        catchState = CatchState.Nothing;
    }

    public void Move(float toTime, float timeScale)
    {
        // Update display
        DisplayDistance = (StartTime - toTime) * timeScale;
        display.SetDistance(DisplayDistance);
        // Update display
        foreach (NotePerformance.PerformanceSegment perfSegment in performance.segments)
        {
            // Display played segments
            if (perfSegment.playsState == NotePerformance.PlayState.PLAYED_CORRECTLY)
            {
                display.Play((perfSegment.time.start - toTime) * timeScale, (perfSegment.time.end - toTime) * timeScale);
            }
            // Display missed segments
            else
            {
                display.Miss((perfSegment.time.start - toTime) * timeScale, (perfSegment.time.end - toTime) * timeScale);
            }
        }
    }

    public void PlayCorrectly(float fromTime, float toTime, float toneError)
    {
        // Clamp times to note time
        fromTime = Mathf.Clamp(fromTime, StartTime, StartTime + Duration);
        toTime = Mathf.Clamp(toTime, StartTime, StartTime + Duration);
        // Update performance
        performance.PlayCorrectly(fromTime, toTime, toneError);
        catchState |= CatchState.Mixed;
    }

    public void PlayWrong(float fromTime, float toTime, float toneError)
    {
        // Clamp times to note time
        fromTime = Mathf.Clamp(fromTime, StartTime, StartTime + Duration);
        toTime = Mathf.Clamp(toTime, StartTime, StartTime + Duration);
        // Update performance
        performance.PlayWrong(fromTime, toTime, toneError);
    }

    public void Miss(float fromTime, float toTime, out FloatSegment missedSegment)
    {
        // Clamp times to note time
        fromTime = Mathf.Clamp(fromTime, StartTime, StartTime + Duration);
        toTime = Mathf.Clamp(toTime, StartTime, StartTime + Duration);
        // Update performance
        missedSegment = performance.Miss(fromTime, toTime).time;
    }

    public void FullCatch()
    {
        // Update display
        display.FullCatch();
        // Update catch state
        catchState = CatchState.All;
    }
}