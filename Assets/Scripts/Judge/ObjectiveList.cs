using System;
using System.Collections.Generic;

[Serializable]
public class ObjectiveList
{
    public EndObjective end;
    public ScoreObjective[] scores;

    public IObjective[] GetObjectives()
    {
        List<IObjective> objectives = new List<IObjective>();
        if (end.enabled) objectives.Add(end);
        if (scores != null) foreach(ScoreObjective o in scores) objectives.Add(o);
        return objectives.ToArray();
    }
}

public interface IObjective
{
    string DisplayName { get; }
}

[Serializable]
public struct EndObjective : IObjective
{
    public bool enabled;

    public string DisplayName => "En entier!";
}

[Serializable]
public struct ScoreObjective : IObjective
{
    public string name;
    public int scoreGoal;

    public string DisplayName => name + " (" + scoreGoal + ")";
}