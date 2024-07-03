using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewGameProgress", menuName = "Trombone Hero/Game Data/Progress")]
public class GameProgress : ScriptableObject
{
    public LevelProgress[] levelProgress;
    public GameContentLock[] contentLocks;

    public void Reset()
    {
        GameContentLibrary content = GameContentLibrary.Current;
        if (content != null)
        {
            levelProgress = Array.ConvertAll(content.levels, l => new LevelProgress(l));
            contentLocks = Array.ConvertAll(content.GetAllContent(), c => new GameContentLock(c));
        }
        else
        {
            levelProgress = new LevelProgress[0];
            contentLocks = new GameContentLock[0];
        }
    }

    public bool MatchesGameContent(GameContentLibrary content)
        => LevelsMatchGameContent(content) && LocksMatchGameContent(content);

    private bool LevelsMatchGameContent(GameContentLibrary content)
    {
        Level[] allLevels = content?.levels;
        if (levelProgress == null || allLevels.Length == 0) return levelProgress.Length == 0;
        foreach (Level l in allLevels) if (Array.FindIndex(levelProgress, progress => progress.levelAsset == l) == -1) return false;
        return true;
    }

    private bool LocksMatchGameContent(GameContentLibrary content)
    {
        ScriptableObject[] allContent = content?.GetAllContent();
        if (allContent == null || allContent.Length == 0) return contentLocks.Length == 0;
        foreach (ScriptableObject c in allContent) if (Array.FindIndex(contentLocks, l => l.contentAsset == c) == -1) return false;
        return true;
    }

    public void Update()
    {
        if (!MatchesGameContent(GameContentLibrary.Current)) Reset();
        AutoUnlock();
    }

    public bool TrySetLock(ScriptableObject contentAsset, bool locked)
    {
        if (contentAsset == null) return false;
        bool foundLock = false;
        for (int i = 0, iend = contentLocks != null ? contentLocks.Length : 0; i < iend; i++)
        {
            if (contentLocks[i].contentAsset == contentAsset)
            {
                foundLock = true;
                contentLocks[i].SetLocked(locked);
            }
        }
        return foundLock;
    }

    public int GetLockCount(out int unlockedCount)
    {
        if (contentLocks == null)
        {
            unlockedCount = 0;
            return 0;
        }
        unlockedCount = Array.FindAll(contentLocks, x => !x.locked).Length;
        return contentLocks.Length;
    }

    public int GetObjectiveCount(out int completedCount)
    {
        completedCount = 0;
        if (levelProgress == null) return 0;
        int objectiveCount = 0;
        foreach (LevelProgress lvl in levelProgress)
        {
            objectiveCount += lvl.ObjectiveCount;
            completedCount += lvl.CompletedObjectivesCount;
        }
        return contentLocks.Length;
    }

    public bool CanUnlock(ScriptableObject contentAsset)
    {
        GetObjectiveCount(out int completedObjectiveCount);
        return CanUnlockWithObjectives(contentAsset, completedObjectiveCount);
    }

    public bool CanUnlockWithObjectives(ScriptableObject contentAsset, int completedObjectiveCount)
    {
        if (Array.Find(contentLocks, contentLock => contentLock.contentAsset == contentAsset).contentAsset == null) return false;
        if (contentAsset is IUnlockableContent) return completedObjectiveCount >= (contentAsset as IUnlockableContent).UnlockTier;
        else return true;
    }

    public void AutoUnlock()
    {
        if (contentLocks == null) return;
        int lockCount = contentLocks.Length;
        GetObjectiveCount(out int completedObjectiveCount);
        for (int i = 0; i < lockCount; i++)
        {
            ScriptableObject contentAsset = contentLocks[i].contentAsset;
            if (contentAsset != null && contentAsset is IUnlockableContent)
            {
                bool canUnlock = CanUnlockWithObjectives(contentAsset, completedObjectiveCount);
                if ((contentAsset as IUnlockableContent).AutoUnlock) contentLocks[i].SetLocked(!canUnlock);
                else if (canUnlock == false) contentLocks[i].SetLocked(true);
            }
        }
    }
}