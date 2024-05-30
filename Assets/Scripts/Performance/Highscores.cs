using System;

[Serializable]
public struct LevelHighscore
{
    public string playerName;
    public string levelID;
    public LevelScoreInfo score;
}

[Serializable]
public struct SessionHighscore
{
    public string playerName;
    public LevelScoreInfo[] scores;
}