using System;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveJudge : MonoBehaviour
{
    public bool showDebug;
    [Header("Components")]
    public MusicPlayer musicPlayer;
    public PerformanceJudge performanceJudge;

    public UnityEvent<ObjectiveInfo> onNewObjectiveComplete;

    private ObjectiveInstance[] objectives;

    private void Reset()
    {
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        performanceJudge = FindObjectOfType<PerformanceJudge>(true);
    }

    private void OnEnable()
    {
        AddListeners();
        EnableGUI();
    }

    private void OnDisable()
    {
        RemoveListeners();
        DisableGUI();
    }

    public void EnableGUI()
    {
        if (performanceJudge?.gui) performanceJudge.gui.Objectives = this;
    }

    public void DisableGUI()
    {
        if (performanceJudge?.gui) performanceJudge.gui.Objectives = null;
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

    public void LoadProgress(bool[] objectiveChecklist)
    {
        int objectiveCount = objectives != null ? objectives.Length : 0;
        int checklistLength = objectiveChecklist != null ? objectiveChecklist.Length : 0;
        for (int i = 0; i < objectiveCount; i++)
        {
            if (objectives[i] == null) continue;
            objectives[i].isComplete = i < checklistLength && objectiveChecklist[i];
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
            if (o.isComplete == false) o.onComplete.AddListener(OnCompleteNewObjective);
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
            o.onComplete.RemoveListener(OnCompleteNewObjective);
            if (musicPlayer) musicPlayer.onMusicEnd.RemoveListener(o.OnMusicEnd);
            if (performanceJudge) performanceJudge.onScore.RemoveListener(o.OnPerformanceJudgeScore);
        }
    }

    private void OnCompleteNewObjective(ObjectiveInfo objectiveInfo)
    {
        if (showDebug) Debug.Log("Completed objective: " + objectiveInfo.type);
        onNewObjectiveComplete.Invoke(objectiveInfo);
    }
}
