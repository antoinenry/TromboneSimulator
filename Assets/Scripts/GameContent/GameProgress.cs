using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewGameProgress", menuName = "Trombone Hero/Game Data/Progress")]
public class GameProgress : ScriptableObject
{
    [CurrentToggle] public bool isCurrent;
    static public GameProgress Current
    {
        get => CurrentAssetsManager.GetCurrent<GameProgress>();
        set => CurrentAssetsManager.SetCurrent(value);
    }

    public LevelProgress[] levels;
    public GameContentLock[] locks;

    public int PlayerXP { get; }

    public int PlayerTier { get; }

    public void Reset()
    {
        GameContentLibrary content = GameContentLibrary.Current;
        if (content != null)
        {
            levels = Array.ConvertAll(content.levels, l => new LevelProgress(l));
            locks = Array.ConvertAll(content.GetAllContent(), c => new GameContentLock(c));
        }
        else
        {
            levels = new LevelProgress[0];
            locks = new GameContentLock[0];
        }
    }

    public void Update()
    {
        if (!MatchesGameContent(GameContentLibrary.Current)) Reset();
    }

    public bool MatchesGameContent(GameContentLibrary content)
        => LevelsMatchGameContent(content) && LocksMatchGameContent(content);

    private bool LevelsMatchGameContent(GameContentLibrary content)
    {
        Level[] allLevels = content?.levels;
        if (levels == null || allLevels.Length == 0) return levels.Length == 0;
        foreach (Level l in allLevels) if (Array.IndexOf(levels, l) == -1) return false;
        return true;
    }

    private bool LocksMatchGameContent(GameContentLibrary content)
    {
        ScriptableObject[] allContent = content?.GetAllContent();
        if (allContent == null || allContent.Length == 0) return locks.Length == 0;
        foreach (ScriptableObject c in allContent) if (Array.IndexOf(locks, c) == -1) return false;
        return true;
    }
}