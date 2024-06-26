using UnityEngine;
using System;

// Storing and updating game content informations
// Including unlockables (levels, instruments, etc)

[CreateAssetMenu(fileName = "NewContentLibrary", menuName = "Trombone Hero/Game Data/Game Content Library")]
public class GameContentLibrary : ScriptableObject
{
    [CurrentToggle] public bool isCurrent;
    static public GameContentLibrary Current
    {
        get => CurrentAssetsManager.GetCurrent<GameContentLibrary>();
        set => CurrentAssetsManager.SetCurrent(value);
    }

    public UnlockableLevel[] levels;
    //public TromboneBuild[] trombones;
    //public Orchestra[] orchestras;
    public UnlockableModifier[] modifiers;

    public string[] GetLevelNames => Array.ConvertAll(levels, l => l?.levelAsset?.name);
}
