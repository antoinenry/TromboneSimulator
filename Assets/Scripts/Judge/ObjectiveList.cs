using System;

[Serializable]
public struct ObjectiveList
{
    [Serializable]
    public struct EndObjective //: IObjective
    {

    }

    [Serializable]
    public struct ScoreObjective //: IObjective
    {
        public string name;
        public int scoreGoal;

    }

    public EndObjective end;
    public ScoreObjective[] scores;

    public int Count => 1 + (scores == null ? 0 : scores.Length);

    public string[] ObjectiveNames
    {
        get
        {
            string[] names = new string[Count];
            names[IndexOf(end)] = "End";
            foreach (ScoreObjective o in scores) names[IndexOf(o)] = o.name;
            return names;
        }
    }

    public int IndexOf(EndObjective objective) => 0;

    public int IndexOf(ScoreObjective objective)
    {
        int scoreIndex = Array.IndexOf(scores, objective);
        if (scoreIndex >= 0) return scoreIndex + IndexOf(end);
        else return -1;
    }

    public int[] IndicesOf(Type objectiveType)
    {
        int[] indices;
        if (objectiveType == typeof(EndObjective))
            indices = new int[] { IndexOf(end) };
        else if (objectiveType == typeof(ScoreObjective))
        {
            int objectifCount = scores.Length;
            int startIndex = IndexOf(end);
            indices = new int[objectifCount];
            for (int i = 0; i < objectifCount; i++) indices[i] = startIndex + i;
        }
        else indices = new int[0];
        return indices;
    }

    //public IObjective[] GetObjectives()
    //{
    //    List<IObjective> objectives = new List<IObjective>();
    //    if (end.enabled) objectives.Add(end);
    //    if (scores != null) foreach(ScoreObjective o in scores) objectives.Add(o);
    //    return objectives.ToArray();
    //}

    //public int Count
    //{
    //    get
    //    {
    //        IObjective[] objectives = GetObjectives();
    //        return objectives != null ? objectives.Length : 0;
    //    }
    //}

    //public IObjective this[int index]
    //    => GetObjectives()[index];
}

//public interface IObjective
//{
//    string DisplayName { get; }
//}