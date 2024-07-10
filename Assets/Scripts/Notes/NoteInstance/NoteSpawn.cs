using UnityEngine;
using System;

[RequireComponent(typeof(NoteBullet))]
public class NoteSpawn : MonoBehaviour, INote
{
    [Flags] public enum CatchState { Nothing = 0, Mixed = 1, All = 3 }

    public NoteBullet display;
    public NoteInfo noteInfo;
    public NotePerformance performance;
    public CatchState catchState;
    public float horizontalCrashDelay;
    public float verticalCrashDelay;

    public float Tone { get => noteInfo.tone; set => noteInfo.tone = value; }
    public float Velocity { get => noteInfo.velocity; set => noteInfo.velocity = value; }
    public float StartTime { get => noteInfo.startTime; set => noteInfo.startTime = value; }
    public float Duration { get => noteInfo.duration; set => noteInfo.duration = value; }

    public Color DisplayColor => display != null ? display.baseColor : Color.magenta;
    public float DisplayLength { get; private set; }
    public float DisplayDistance { get; private set; }
    public float DisplayStart => display != null ? DisplayDistance + display.distanceOffset : 0f;
    public float DisplayEnd => display != null ? DisplayDistance + DisplayLength + display.distanceOffset : 0f;

    private void Awake()
    {
        display = GetComponent<NoteBullet>();
    }

    public void Init(float time, float timeScale, Color color, float incomingTime)//, bool linkToNextNote = false)
    {
        // Init display
        //display.SetSprites(linkToNextNote);
        DisplayLength = Duration * timeScale;
        display.SetLength(DisplayLength);
        DisplayDistance = (StartTime - time) * timeScale;
        display.SetDistance(DisplayDistance);
        display.baseColor = color;
        display.incomingDistance = incomingTime * timeScale;
        // Init catch state
        catchState = CatchState.Nothing;
        // Init crash distance
        horizontalCrashDelay = float.PositiveInfinity;
        verticalCrashDelay = float.PositiveInfinity;
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
                display.Play((perfSegment.time.start - toTime) * timeScale, (perfSegment.time.end - toTime) * timeScale, perfSegment.toneAccuracy);
            // Display missed segments
            else
                display.Miss((perfSegment.time.start - toTime) * timeScale, (perfSegment.time.end - toTime) * timeScale);
        }
        // Cut crashed segments
        if (toTime > noteInfo.startTime + horizontalCrashDelay)
            display.Cut(float.NegativeInfinity, -horizontalCrashDelay * timeScale, horizontal: true, vertical: false);
        if (toTime > noteInfo.startTime + verticalCrashDelay)
            display.Cut(float.NegativeInfinity, -verticalCrashDelay * timeScale, horizontal: false, vertical: true);
    }

    public void PlayCorrectly(float fromTime, float toTime, float toneAccurcay)
    {
        // Clamp times to note time
        fromTime = Mathf.Clamp(fromTime, StartTime, StartTime + Duration);
        toTime = Mathf.Clamp(toTime, StartTime, StartTime + Duration);
        // Update performance
        performance.PlayCorrectly(fromTime, toTime, toneAccurcay);
        catchState |= CatchState.Mixed;
    }

    public void PlayWrong(float fromTime, float toTime, float toneAccuracy)
    {
        // Clamp times to note time
        fromTime = Mathf.Clamp(fromTime, StartTime, StartTime + Duration);
        toTime = Mathf.Clamp(toTime, StartTime, StartTime + Duration);
        // Update performance
        performance.PlayWrong(fromTime, toTime, toneAccuracy);
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

    public void Crash(float crashDistance, bool horizontal = true, bool vertical = true)
    {
        //display.Cut(float.NegativeInfinity, -horizontalCrashDelay * timeScale, horizontal: true, vertical: false);
        display.Cut(float.NegativeInfinity, crashDistance, horizontal: true, vertical: false);
    }

    public void SetVisible(bool x, bool y)
    {
        display.SetVisible(x, y);
    }

    public bool TryLinkToNextNote(NoteSpawn otherNote) => display != null && display.TryLinkToNextNote(otherNote?.display);
}