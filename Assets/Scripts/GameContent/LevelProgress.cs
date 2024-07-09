using System;

[Serializable]
public struct LevelProgress
{
    public Level levelAsset;
    public bool[] checkList;

    public LevelProgress(Level level)
    {
        levelAsset = level;
        int objectiveCount = level?.objectives != null ? level.objectives.Length : 0;
        checkList = new bool[objectiveCount];
    }

    public int ObjectiveCount => levelAsset?.objectives != null ? levelAsset.objectives.Length : 0;
    public string[] ObjectiveNames => levelAsset?.objectives != null ? Array.ConvertAll(levelAsset.objectives, o => o.type) : new string[0];

    public int CompletedObjectivesCount
    {
        get
        {
            int counter = 0;
            for (int i = 0, iend = CorrectChecklistLength(); i < iend; i++) if (checkList[i]) counter++;
            return counter;
        }
    }

    public bool TryCheckObjective(int index, bool value = true)
    {
        if (index < 0 || index >= CorrectChecklistLength()) return false;
        checkList[index] = value;
        return true;
    }

    public bool TryCheckObjective(ObjectiveInfo objective, bool value = true)
    {
        if (levelAsset == null) return false;
        return TryCheckObjective(Array.IndexOf(levelAsset.objectives, objective), value);
    }

    public void CheckAllObjectives()
    {
        if (checkList != null) checkList = Array.ConvertAll(checkList, c => true);
    }

    public void UncheckAllObjectives()
    {
        if (checkList != null) checkList = Array.ConvertAll(checkList, c => false);
    }

    private int CorrectChecklistLength()
    {
        int correctLength = ObjectiveCount;
        if (checkList == null) checkList = new bool[correctLength];
        else if (checkList.Length != correctLength) Array.Resize(ref checkList, correctLength);
        return correctLength;
    }
}