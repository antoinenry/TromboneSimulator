using System;
using UnityEngine;

public class ObjectiveJudge : MonoBehaviour
{
    public bool showDebug;
    [Header("Components")]
    public MusicPlayer musicPlayer;
    public PerformanceJudge performanceJudge;

    private ObjectiveInstance[] objectives;

    private void Reset()
    {
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        performanceJudge = FindObjectOfType<PerformanceJudge>(true);
    }

    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public void LoadObjectives(ObjectiveInfo[] objectiveInfos)
    {
        RemoveListeners();
        objectives = Array.ConvertAll(objectiveInfos, info => ObjectiveInstance.NewObjective(info));
        AddListeners();
        if (showDebug)
        {
            Debug.Log("Loading objectives:");
            foreach (object o in objectives) Debug.Log(o != null ? o.GetType() : "null");
        }
    }

    public void UnloadObjectives()
    {
        RemoveListeners();
        objectives = null;
    }

    public ObjectiveInfo[] GetCompletedObjectives()
    {
        ObjectiveInstance[] completedInstances = Array.FindAll(objectives, o => o != null && o.isComplete);
        return Array.ConvertAll(completedInstances, o => o.GetInfo());
    }

    private void AddListeners()
    {
        if (objectives == null) return;
        foreach (ObjectiveInstance o in objectives)
        {
            if (o == null) continue;
            o.onComplete.AddListener(OnCompleteObjective);
            if (musicPlayer) musicPlayer.onMusicEnd.AddListener(o.OnMusicEnd);
            if (performanceJudge) performanceJudge.onScore.AddListener(o.OnPerformanceJudgeScore);
        }
    }

    private void RemoveListeners()
    {
        if (objectives == null) return;
        foreach (ObjectiveInstance o in objectives)
        {
            if (o == null) continue;
            o.onComplete.RemoveListener(OnCompleteObjective);
            if (musicPlayer) musicPlayer.onMusicEnd.RemoveListener(o.OnMusicEnd);
            if (performanceJudge) performanceJudge.onScore.RemoveListener(o.OnPerformanceJudgeScore);
        }
    }

    private void OnCompleteObjective(ObjectiveInfo objectiveInfo)
    {
        if (showDebug) Debug.Log("Completed objective: " + objectiveInfo.type);
    }
}
