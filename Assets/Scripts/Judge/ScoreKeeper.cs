using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewScoreKeeper", menuName = "Trombone Hero/Game Data/Score Keeper")]
public class ScoreKeeper : ScriptableObject
{
    [CurrentToggle] public bool isCurrent;
    public string playerName;
    public List<LevelScoreInfo> score;
}
