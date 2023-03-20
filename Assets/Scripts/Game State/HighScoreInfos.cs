using System;

[Serializable]
public struct HighScoreInfos
{
    public string playerName;
    public int score;

    public HighScoreInfos(string name, int scr)
    {
        playerName = name;
        score = scr;
    }
}