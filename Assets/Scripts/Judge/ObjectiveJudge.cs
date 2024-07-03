using UnityEngine;
using System;

public class ObjectiveJudge : MonoBehaviour
{
    public bool showDebug;
    [Header("Components")]
    public MusicPlayer musicPlayer;
    public PerformanceJudge performanceJudge;

    private Objective[] objectives;

    private void Reset()
    {
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        performanceJudge = FindObjectOfType<PerformanceJudge>(true);
    }

    private void OnEnable()
    {
        if (objectives == null) objectives = new Objective[0];
        if (musicPlayer) foreach (Objective o in objectives) musicPlayer.onMusicEnd.AddListener(o.OnMusicEnd);
        if (performanceJudge) foreach (Objective o in objectives) performanceJudge.onScore.AddListener(o.OnPerformanceJudgeScore);
    }

    private void OnDisable()
    {
        if (objectives == null) objectives = new Objective[0];
    }

    public void LoadObjectives(SerializableObjectiveInfo[] objectiveInfos)
    {
        objectives = Array.ConvertAll(objectiveInfos, info => Objective.NewObjective(info));
        if (showDebug)
        {
            Debug.Log("Loaded objectives:");
            foreach (object o in objectives) Debug.Log(o != null ? o.GetType() : "null");
        }
    }
}
