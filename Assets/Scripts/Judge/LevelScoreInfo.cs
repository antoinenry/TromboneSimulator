using System;
using UnityEngine;

[Serializable]
public struct LevelScoreInfo
{
    public int baseScore;
    [Header("Performance")]
    public int correctNoteCount;
    public int totalNoteCount;
    public float accuracyAverage;
    public int bestCombo;
    [Header("Bonuses")]
    public float accuracyBonusMultiplier;
    public float playedNotesBonusMultiplier;
    public float bestComboBonusMultiplier;

    public int PlayedNoteBonus => Mathf.RoundToInt(correctNoteCount * playedNotesBonusMultiplier);
    public int AccuracyBonus => Mathf.RoundToInt(accuracyAverage * accuracyBonusMultiplier);
    public int ComboBonus => Mathf.RoundToInt(bestCombo * bestComboBonusMultiplier);
    public int Total => baseScore + PlayedNoteBonus + AccuracyBonus + ComboBonus;

    static public LevelScoreInfo Zero => new LevelScoreInfo()
    {
        baseScore = 0,
        correctNoteCount = 0,
        totalNoteCount = 0,
        accuracyAverage = 0f,
        bestCombo = 0,
        accuracyBonusMultiplier = 0f,
        playedNotesBonusMultiplier = 0f,
        bestComboBonusMultiplier = 0f
    };

    static public LevelScoreInfo None => new LevelScoreInfo()
    {
        baseScore = -1,
        correctNoteCount = -1,
        totalNoteCount = -1,
        accuracyAverage = -1f,
        bestCombo = -1,
        accuracyBonusMultiplier = -1f,
        playedNotesBonusMultiplier = -1f,
        bestComboBonusMultiplier = -1f
    };

    public void ClearPerformance()
    {
        baseScore = 0;
        correctNoteCount = 0;
        totalNoteCount = 0;
        accuracyAverage = 0f;
        bestCombo = 0;
    }
}