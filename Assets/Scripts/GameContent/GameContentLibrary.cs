using UnityEngine;
using System;

// Storing and updating game content informations
// Including unlockables (levels, instruments, etc)

[CreateAssetMenu(fileName = "NewContentLibrary", menuName = "Trombone Hero/Game Data/Game Content Library")]
public class GameContentLibrary : SingleScriptableObject
{
    #region SingleScriptableObject implementation (only one active GameContentLibrary at a time)
    protected override SingleScriptableObject CurrentObject { get => Current; set => Current = value as GameContentLibrary; }
    static public GameContentLibrary Current;
    #endregion

    public Level[] levels;
    public TromboneBuild[] trombones;
    public Orchestra[] orchestras;

    public string[] GetLevelNames => Array.ConvertAll(levels, l => l != null ? l.name : null);
}
