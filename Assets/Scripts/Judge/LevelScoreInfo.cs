using System;
using UnityEngine;

[Serializable]
public struct LevelScoreInfo
{
    public int baseScore;
    public int correctNoteCount;
    public int totalNoteCount;
    public float accuracyAverage;
    public int bestCombo;

    public int PlayedNoteBonus => correctNoteCount * 100;
    public int AccuracyBonus => Mathf.RoundToInt(accuracyAverage * 10000f);
    public int ComboBonus => bestCombo * 100;
    public int Total => baseScore + PlayedNoteBonus + AccuracyBonus + ComboBonus;

    static public LevelScoreInfo Zero => new LevelScoreInfo()
    {
        baseScore = 0,
        correctNoteCount = 0,
        totalNoteCount = 0,
        accuracyAverage = 0f,
        bestCombo = 0
    };

    static public LevelScoreInfo None => new LevelScoreInfo()
    {
        baseScore = -1,
        correctNoteCount = -1,
        totalNoteCount = -1,
        accuracyAverage = -1f,
        bestCombo = -1
    };
}