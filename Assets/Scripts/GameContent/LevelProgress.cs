using System;
using UnityEditor;

[Serializable]
public struct LevelProgress
{
    public Level levelAsset;
    public bool[] checklist;

    public LevelProgress(Level level, bool[] checklist)
    {
        levelAsset = level;
        int objectiveCount = level?.objectives != null ? level.objectives.Length : 0;
        this.checklist = new bool[objectiveCount];
        int checklistLength = checklist != null ? checklist.Length : 0;
        for (int i = 0; i < objectiveCount; i++)
            this.checklist[i] = i < checklistLength ? checklist[i] : false;
    }

    public LevelProgress(Level level, ObjectiveInfo[] completedObjectives = null)
    {
        levelAsset = level;
        int objectiveCount = level?.objectives != null ? level.objectives.Length : 0;
        checklist = new bool[objectiveCount];
        if (completedObjectives != null) foreach (ObjectiveInfo o in completedObjectives) TryCheckObjective(o);
    }

    public LevelProgress Clone() => new(levelAsset, checklist);

    public int ObjectiveCount => levelAsset?.objectives != null ? levelAsset.objectives.Length : 0;
    public string[] ObjectiveNames => levelAsset?.objectives != null ? Array.ConvertAll(levelAsset.objectives, o => o.name) : new string[0];
    public string[] ObjectiveLongNames => levelAsset?.objectives != null ? Array.ConvertAll(levelAsset.objectives, o => o.LongName) : new string[0];

    public int CompletedObjectivesCount
    {
        get
        {
            int counter = 0;
            for (int i = 0, iend = CorrectChecklistLength(); i < iend; i++) if (checklist[i]) counter++;
            return counter;
        }
    }

    public int GetObjectiveIndex(ObjectiveInfo o)
    {
        if (levelAsset == null) return -1;
        return Array.IndexOf(levelAsset.objectives, o);
    }

    public bool TryCheckObjective(int index, bool value = true)
    {
        if (index < 0 || index >= CorrectChecklistLength()) return false;
        checklist[index] = value;
        return true;
    }

    public bool TryCheckObjective(ObjectiveInfo objective, bool value = true)
    {
        if (levelAsset == null) return false;
        int objectiveIndex = GetObjectiveIndex(objective);
        return TryCheckObjective(objectiveIndex, value);
    }

    public bool TryCheckObjectives(ObjectiveInfo[] objective, bool value = true)
    {
        bool success = true;
        foreach (ObjectiveInfo obj in objective)
        {
            if (TryCheckObjective(obj, value) == false) success = false;
        }
        return success;
    }

    public void CheckAllObjectives()
    {
        if (checklist != null) checklist = Array.ConvertAll(checklist, c => true);
    }

    public void UncheckAllObjectives()
    {
        if (checklist != null) checklist = Array.ConvertAll(checklist, c => false);
    }

    private int CorrectChecklistLength()
    {
        int correctLength = ObjectiveCount;
        if (checklist == null) checklist = new bool[correctLength];
        else if (checklist.Length != correctLength) Array.Resize(ref checklist, correctLength);
        return correctLength;
    }
}