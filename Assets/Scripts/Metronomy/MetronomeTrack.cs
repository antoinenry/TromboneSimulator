//using System;
//using System.Linq;
//using System.Collections.Generic;
//using UnityEngine;

//[Serializable]
//public struct MetronomeTrack
//{   
//    //private MetronomeTrackSection[] tempoSections;
//    private List<float> beatTimes;
//    private List<float> barTimes;

//    //public int Sections => tempoSections != null ? tempoSections.Length : 0;
//    //public float FirstSectionStart => Sections > 0 ? tempoSections[0].tempo.time : 0f;
//    //public float FirstSectionEnd
//    //{
//    //    get
//    //    {
//    //        switch(Sections)
//    //        {
//    //            case 0: return 0f;
//    //            case 1: return tempoSections[0].tempo.SecondsPerBar;
//    //            default: return tempoSections[1].tempo.time;
//    //        }
//    //    }
//    //}
//    //public float LastSectionStart => Sections > 0 ? tempoSections[Sections - 1].tempo.time : 0f;
//    //public float LastSectionEnd => Sections > 0 ? tempoSections[Sections - 1].tempo.SecondsPerBar : 0f;
//    //public float[] Bars => barTimes != null ? barTimes.ToArray() : null;
//    //public float[] Beats => beatTimes != null ? beatTimes.ToArray() : null;

//    //public void SetTempo(params TempoInfo[] tempoInfos)
//    //{
//    //    // Default values
//    //    tempoSections = new MetronomeTrackSection[0];
//    //    beatTimes = new List<float>();
//    //    barTimes = new List<float>();
//    //    // Read tempos
//    //    if (tempoInfos != null && tempoInfos.Length > 0)
//    //    {
//    //        // Ensure tempo infos are sorted by time
//    //        TempoInfo[] orderedTempos =  tempoInfos.OrderBy(t => t.time).ToArray();
//    //        // For each tempo change, create a tempo section
//    //        int sectionCount = tempoInfos.Length;
//    //        tempoSections = new MetronomeTrackSection[sectionCount];
//    //        // First section is an exception (initializes offset values)
//    //        tempoSections[0] = new MetronomeTrackSection()
//    //        {
//    //            // A tempo section contains the same infos as Tempo info
//    //            tempo = new(orderedTempos[0]),
//    //            // Plus time offsets for bar placements
//    //            barOffset = 0f
//    //        };
//    //        // Other sections rely on previous sections to set offset values
//    //        for (int i = 1; i < sectionCount; i++)
//    //        {
//    //            MetronomeTrackSection previousSection = tempoSections[i - 1];
//    //            TempoInfo sectionTempo = orderedTempos[i];
//    //            tempoSections[i] = new MetronomeTrackSection()
//    //            {
//    //                tempo = new(sectionTempo),
//    //                barOffset = -previousSection.SecondsSinceLastBar(sectionTempo.time) / sectionTempo.SecondsPerBar
//    //            };
//    //        }
//    //        // Locate beats and bars for more efficient reading
//    //        float[] beats, bars;
//    //        for (int i = 0; i < sectionCount - 1; i++)
//    //        {
//    //            tempoSections[i].GetTimes(tempoSections[i].tempo.time, tempoSections[i + 1].tempo.time, out beats, out bars);
//    //            barTimes.AddRange(bars);
//    //            beatTimes.AddRange(beats);
//    //        }
//    //        // Exception for last section: since it has virtually no end time, we set duration, bars and beats for the duration of one bar only
//    //        MetronomeTrackSection lastSection = tempoSections[sectionCount - 1];
//    //        float barDuration = lastSection.tempo.SecondsPerBar;
//    //        lastSection.GetTimes(lastSection.tempo.time, lastSection.tempo.time + lastSection.tempo.SecondsPerBar, out beats, out bars);
//    //        barTimes.AddRange(bars);
//    //        beatTimes.AddRange(beats);
//    //    }
//    //}

//    //public int CountBars(float fromTime, float toTime)
//    //{
//    //    int barCount = 0;
//    //    // Ensure parameters are in the right order
//    //    if (fromTime > toTime)
//    //    {
//    //        float switchValue = toTime;
//    //        toTime = fromTime;
//    //        fromTime = switchValue;
//    //    }
//    //    // Start at 'fromTime' tempo section
//    //    int sectionIndex = GetTempoSectionIndex(fromTime);
//    //    // Ponctual time exception
//    //    if (fromTime == toTime)
//    //        return barTimes.Contains(fromTime) ? 1 : 0;
//    //    // Navigate from sections to sections
//    //    float countFromTime = fromTime;
//    //    float countToTime;
//    //    while (sectionIndex != -1 && sectionIndex < Sections)
//    //    {
//    //        // Get section end time
//    //        float sectionEnd;
//    //        if (sectionIndex != Sections - 1) sectionEnd = tempoSections[sectionIndex + 1].tempo.time;
//    //        // Consider last section to last forever
//    //        else sectionEnd = toTime;
//    //        // Count bars in this laps of time
//    //        countToTime = Mathf.Min(sectionEnd, toTime);
//    //        MetronomeTrackSection section = tempoSections[sectionIndex];
//    //        barCount += Mathf.FloorToInt((countToTime - countFromTime - section.BarOffsetSeconds) / section.tempo.SecondsPerBar);
//    //        // Move to next section
//    //        if (countToTime < toTime)
//    //        {
//    //            countFromTime = countToTime;
//    //            sectionIndex++;
//    //        }
//    //        // Or end counting
//    //        else break;
//    //    }
//    //    return barCount;
//    //}

//    //public int GetTempoSectionIndex(float atTime)
//    //{
//    //    int sectionCount = tempoSections != null ? tempoSections.Length : 0;
//    //    if (sectionCount == 0) return -1;
//    //    // Exception: atTime is before first tempo section
//    //    if (tempoSections[0].tempo.time > atTime) return 0;
//    //    // Move to next section until atTime is in section range
//    //    for (int i = 0; i < sectionCount - 1; i++)
//    //        if (tempoSections[i + 1].tempo.time > atTime) return i;
//    //    // Exception: atTime is after last tempo section
//    //    return sectionCount - 1;
//    //}

//    //public float GetBarProgress(float atTime)
//    //{
//    //    int sectionIndex = GetTempoSectionIndex(atTime);
//    //    return GetBarProgress(atTime, sectionIndex);
//    //}

//    //public float GetBarProgress(float atTime, int sectionIndex)
//    //{
//    //    if (sectionIndex >= 0 && sectionIndex < Sections)
//    //    {
//    //        MetronomeTrackSection section = tempoSections[sectionIndex];
//    //        float progressSeconds = section.SecondsSinceLastBar(atTime);
//    //        float secondsPerBar = section.tempo.SecondsPerBar;
//    //        if (secondsPerBar != 0f) return Mathf.Repeat(progressSeconds / secondsPerBar, 1f);
//    //        else return 0f;
//    //    }
//    //    else return 0f;
//    //}

//    //public TempoInfo GetTempo(int sectionIndex)
//    //{
//    //    if (sectionIndex >= 0 && sectionIndex < Sections) return tempoSections[sectionIndex].tempo;
//    //    else return new TempoInfo();
//    //}

//    //public TempoInfo GetTempo(float atTime) =>  GetTempo(GetTempoSectionIndex(atTime));
//}