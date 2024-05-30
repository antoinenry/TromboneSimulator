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

    public Level[] levels;
    public TromboneBuild[] trombones;
    public Orchestra[] orchestras;

    public string[] GetLevelNames => Array.ConvertAll(levels, l => l != null ? l.name : null);
    public string[] GetTromboneNames => Array.ConvertAll(trombones, l => l != null ? l.name : null);
}
