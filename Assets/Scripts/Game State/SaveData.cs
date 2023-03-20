using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    [SerializeField] public LevelScoreInfo[] currentScoreDetail;
    [SerializeField] public HighScoreInfos[] levelHighScores;
    [SerializeField] public HighScoreInfos[] arcadeHighScores;
    [SerializeField] public GameSettingsInfo settings;
}