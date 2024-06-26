using System;

[Serializable]
public struct LevelProgress
{
    public Level levelAsset;
    public bool[] objectiveCompletions;

    public LevelProgress(Level level)
    {
        levelAsset = level;
        int objectiveCount = level != null ? level.objectives.GetObjectives().Length : 0;
        objectiveCompletions = new bool[objectiveCount];
    }
}