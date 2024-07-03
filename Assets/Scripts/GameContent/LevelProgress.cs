using System;

[Serializable]
public struct LevelProgress
{
    public Level levelAsset;
    public bool[] checkList;

    public LevelProgress(Level level)
    {
        levelAsset = level;
        int objectiveCount = level != null ? level.objectiveList.Count : 0;
        checkList = new bool[objectiveCount];
    }

    public int ObjectiveCount => levelAsset != null ? levelAsset.objectiveList.Count : 0;
    public string[] ObjectiveNames => levelAsset?.objectiveList.ObjectiveNames;

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

    private int CorrectChecklistLength()
    {
        int correctLength = ObjectiveCount;
        if (checkList == null) checkList = new bool[correctLength];
        else if (checkList.Length != correctLength) Array.Resize(ref checkList, correctLength);
        return correctLength;
    }
}