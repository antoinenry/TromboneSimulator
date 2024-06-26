using UnityEngine;
using System;

public class ObjectiveJudge : MonoBehaviour
{
    [Serializable]
    public class ObjectiveProgress
    {
        public string name;
        public bool completed;

        public readonly IObjective ObjectiveInfo;

        public ObjectiveProgress(IObjective objective)
        {
            ObjectiveInfo = objective;
            name = objective.DisplayName;
            completed = false;
        }
    }

    public ObjectiveProgress[] progress;

    private MusicPlayer musicPlayer;
    private PerformanceJudge performanceJudge;

    private void Awake()
    {
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        performanceJudge = FindObjectOfType<PerformanceJudge>(true);
    }

    private void OnEnable()
    {
        if (musicPlayer) musicPlayer.onPlayerUpdate.AddListener(OnMusicPlayerUpdate);
        if (performanceJudge) performanceJudge.onScore.AddListener(OnPerformanceJudgeScore);
    }

    private void OnDisable()
    {
        if (musicPlayer) musicPlayer.onPlayerUpdate.RemoveListener(OnMusicPlayerUpdate);
        if (performanceJudge) performanceJudge.onScore.RemoveListener(OnPerformanceJudgeScore);
    }

    public void LoadObjectiveList(ObjectiveList list)
    {
        IObjective[] objectives = list?.GetObjectives();
        if (objectives == null) progress = new ObjectiveProgress[0];
        else progress = Array.ConvertAll(objectives, o => new ObjectiveProgress(o));
    }

    public void UnloadObjectiveList()
    {
        progress = new ObjectiveProgress[0];
    }

    private void OnMusicPlayerUpdate()
    {
        bool musicEnd = musicPlayer != null && musicPlayer.CurrentPlayTime >= musicPlayer.MusicDuration;
        foreach (ObjectiveProgress o in progress)
        {
            if (o != null && o.ObjectiveInfo is EndObjective)
                o.completed = musicEnd;
        }
    }

    private void OnPerformanceJudgeScore(float score)
    {
        foreach (ObjectiveProgress o in progress)
        {
            if (o != null && o.ObjectiveInfo is ScoreObjective)
                o.completed = score >= ((ScoreObjective)o.ObjectiveInfo).scoreGoal;
        }
    }
}
