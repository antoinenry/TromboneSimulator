using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewContentLibrary", menuName = "Trombone Hero/Game Content Library")]
public class GameContentLibrary : ScriptableObject
{
    [CurrentToggle] public bool isCurrent;
    static public GameContentLibrary Current
    {
        get => CurrentAssetsManager.GetCurrent<GameContentLibrary>();
        set => CurrentAssetsManager.SetCurrent(value);
    }

    public Level[] levels;
    public TromboneBuildModifier[] modifiers;
    public ObjectMethodCaller caller = new ObjectMethodCaller("SortByUnlockTier");
    //public TromboneBuild[] trombones;
    //public Orchestra[] orchestras;

    public string[] GetLevelNames()
        => Array.ConvertAll(levels, l => l?.name);

    public ScriptableObject[] GetAllContent()
    {
        int levelCount = levels != null ? levels.Length : 0;
        int modifierCount = modifiers != null ? modifiers.Length : 0;
        ScriptableObject[] allContent = new ScriptableObject[levelCount + modifierCount];
        Array.Copy(levels, allContent, levelCount);
        Array.Copy(modifiers, 0, allContent, levelCount, modifierCount);
        return allContent;
    }

    public void SortByUnlockTier()
    {
        UnlockTierComparer comparer = new();
        if (levels != null) Array.Sort(levels, comparer);
        if (modifiers != null) Array.Sort(modifiers, comparer);
    }
}
