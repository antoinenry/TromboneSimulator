using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MetronomeTrackSection
{
    //public TempoInfo tempo;
    //[Range(0f, 1f)] public float barOffset;

    //public float BarOffsetSeconds => barOffset * tempo.SecondsPerBar;

    //public float SecondsSinceLastBar(float time) => (time - (tempo.time + BarOffsetSeconds)) % tempo.SecondsPerBar;

    //public void GetTimes(float fromTime, float toTime, out float[] beats, out float[] bars)
    //{
    //    List<float> beatTimes = new List<float>();
    //    List<float> barTimes = new List<float>();

    //    float secondsPerBeat = tempo.secondsPerQuarterNote * tempo.quarterNotesPerBeat;
    //    float secondsSinceLastBar = SecondsSinceLastBar(fromTime);
    //    int beatCounter = Mathf.FloorToInt(secondsSinceLastBar / secondsPerBeat);
    //    float beatsPerBar = tempo.BeatsPerBar;
    //    float beatTime = secondsSinceLastBar % secondsPerBeat;
    //    while (beatTime < toTime)
    //    {
    //        if (beatCounter >= beatsPerBar)
    //        {
    //            barTimes.Add(beatTime);
    //            beatCounter = 0;
    //        }
    //        beatTimes.Add(beatTime);
    //        beatTime += secondsPerBeat;
    //        beatCounter += 1;
    //    }
    //    beats = beatTimes.ToArray();
    //    bars = barTimes.ToArray();
    //}
}