using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelCollection", menuName = "Trombone Hero/Game Data/Level Collection")]
public class LevelCollection : ScriptableObject
{
    public Level[] levels;

    public int Count => levels != null ? levels.Length : 0;
    public string[] LevelNames => levels != null ? Array.ConvertAll(levels, lvl => lvl.Name) : new string[0];
}