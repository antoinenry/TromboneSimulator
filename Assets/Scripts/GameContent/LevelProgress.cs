using System;

[Serializable]
public struct LevelProgress
{
    public Level levelAsset;
    public bool[] objectiveCompletion;

    public LevelProgress(Level level)
    {
        levelAsset = level;
        int objectiveCount = level != null ? level.objectives.GetObjectives().Length : 0;
        objectiveCompletion = new bool[objectiveCount];
    }

    public void Reset()
    {
        int objectiveCount = levelAsset != null ? levelAsset.objectives.GetObjectives().Length : 0;
        objectiveCompletion = new bool[objectiveCount];
    }

    public int GetObjectiveProgress(out IObjective[] objectives, out bool[] completion)
    {
        int objectiveCount = levelAsset != null ? levelAsset.ObjectiveCount : 0;
        objectives = new IObjective[objectiveCount];
        completion = new bool[objectiveCount];
        if (objectiveCompletion == null || objectiveCompletion.Length != objectiveCount) Reset();
        if (objectiveCount == 0) return 0;
        Array.Copy(levelAsset.objectives.GetObjectives(), objectives, objectiveCount);
        Array.Copy(objectiveCompletion, completion, objectiveCount);
        return objectiveCount;
    }

    public int GetObjectiveProgress(out int completedCount)
    {
        int objectiveCount = levelAsset != null ? levelAsset.ObjectiveCount : 0;
        if (objectiveCompletion == null || objectiveCompletion.Length != objectiveCount) Reset();
        completedCount = 0;
        if (objectiveCount == 0) return 0;
        for (int  i = 0; i < levelAsset.ObjectiveCount; i++) if (objectiveCompletion[i]) completedCount++;
        return objectiveCount;
    }

    public bool TryCheckObjective(int index, bool value = true)
    {
        if (index < 0 || objectiveCompletion == null || index > objectiveCompletion.Length) return false;
        objectiveCompletion[index] = value;
        return true;
    }
}