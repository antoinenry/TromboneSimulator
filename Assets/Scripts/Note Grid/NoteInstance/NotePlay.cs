using UnityEngine;
using System;

public class NotePlay : INote
{
    public NoteInfo noteInfo;
    public int noteIndex;

    public float Tone { get => noteInfo.tone; set => noteInfo.tone = value; }
    public float Velocity { get => noteInfo.velocity; set => noteInfo.velocity = value; }
    public float StartTime { get => noteInfo.startTime; set => noteInfo.startTime = value; }
    public float Duration { get => noteInfo.duration; set => noteInfo.duration = value; }

    public NotePlay(NoteInfo info, int index)
    {
        noteInfo = info;
        noteIndex = index;
    }
}