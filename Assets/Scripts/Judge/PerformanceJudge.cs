using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PerformanceJudge : MonoBehaviour
{
    public bool showDebug;
    [Header("Components")]
    public NoteCatcher noteCatcher;
    public NoteCrasher noteCrasher;
    public PerformanceGUI gui;
    [Header("Difficulty")]
    public float maxHealth = 1f;
    public float scoringRate = 10f;
    public float damageRate = .2f;
    public float accuracyRounding = 10f;
    [Header("Performance")]
    public List<NotePerformance> performanceDetail;
    public float score;
    public int combo;
    public float health;
    [Header("Events")]
    public UnityEvent<float> onScore;
    public UnityEvent<float,float,float> onHealth;
    public UnityEvent<NoteSpawn, float, float> onCorrectNote;
    public UnityEvent<NoteSpawn> onWrongNote;
    public UnityEvent<NoteSpawn> onMissNote;
    public UnityEvent<NoteSpawn, float> onNotePerformanceEnd;
    public UnityEvent<int> onNoteCombo;

    private void OnEnable()
    {
        EnableDetection();
        EnableGUI();
    }

    private void OnDisable()
    {
        DisableDetection();
        DisableGUI();
    }

    public void EnableGUI()
    {
        if (gui) gui.Judge = this;
    }

    public void DisableGUI()
    {
        if (gui && gui.Judge == this) gui.Judge = null;
    }

    public void EnableDetection()
    {
        if (noteCatcher)
        {
            noteCatcher.onCorrectNote.AddListener(OnPlayCorrectNote);
            noteCatcher.onWrongNote.AddListener(OnPlayWrongNote);
            noteCatcher.onMissNote.AddListener(OnMissNote);
            noteCatcher.onNoteCatchEnd.AddListener(OnNoteExit);
        }
        if (noteCrasher)
        {
            noteCrasher.onHorizontalCrash.AddListener(OnNoteCrash);
            noteCrasher.onVerticalCrash.AddListener(OnNoteCrash);
        }
    }

    public void DisableDetection()
    {
        if (noteCatcher != null)
        {
            noteCatcher.onCorrectNote.RemoveListener(OnPlayCorrectNote);
            noteCatcher.onWrongNote.RemoveListener(OnPlayWrongNote);
            noteCatcher.onMissNote.RemoveListener(OnMissNote);
            noteCatcher.onNoteCatchEnd.RemoveListener(OnNoteExit);
        }
        if (noteCrasher != null)
        {
            noteCrasher.onHorizontalCrash.RemoveListener(OnNoteCrash);
            noteCrasher.onVerticalCrash.RemoveListener(OnNoteCrash);
        }
    }
    public void ResetPerformance()
    {
        // Reset values
        performanceDetail = new List<NotePerformance>();
        score = 0f;
        health = maxHealth;
        combo = 0;
        // Reset GUI
        if (gui) gui.ResetDisplay(maxHealth);
    }

    public void LevelSetup(SheetMusic lvl, TromboneCore trombone)
    {
        // Prepare level
        if (lvl != null) performanceDetail = new List<NotePerformance>(lvl.GetPartLength(trombone?.Sampler?.name));
        // Reset
        ResetPerformance();
    }

    public void SetScore(float value)
    {
        score = value;
        onScore.Invoke(score);
    }

    public void AddNotePerformance(NoteSpawn instance)
    {
        // Add note performance
        performanceDetail.Add(instance.performance);
        // Add points to score
        float noteScore = GetNoteScore(instance.performance);
        SetScore(score + noteScore * combo);
        // Increase combo if note was fully played
        bool fullPlay = instance.performance.CorrectTime == instance.Duration;
        if (fullPlay) SetCombo(combo + 1);
        // Else break combo
        else SetCombo(0);
        onNotePerformanceEnd.Invoke(instance, noteScore);
    }

    public void SetCombo(int value)
    {
        combo = value;
        onNoteCombo.Invoke(combo);
    }

    public void TakeDamage(float damagePoints)
    {
        if (showDebug) Debug.Log("Take " + damagePoints + " damage");
        health = Mathf.Max(health - damagePoints, 0f);
        onHealth.Invoke(health, maxHealth, -damagePoints);
    }

    public void HealDamage(float healPoints)
    {
        if (showDebug) Debug.Log("Heal " + healPoints + " hp");
        health = Mathf.Min(health + healPoints, maxHealth);
        onHealth.Invoke(health, maxHealth, healPoints);
    }

    public float GetNoteAccuracy(NotePerformance notePerformance, bool rounded = true)
    {
        if (notePerformance.CorrectTime == 0f) return 0f;
        float accuracy = NotePerformance.PerformanceSegment.AccuracyAverage(notePerformance.CorrectSegments);
        //float accuracy = noteCatcher.toneTolerance != 0f ? 1f - averageError / noteCatcher.toneTolerance : 1f;
        if (rounded == true && accuracyRounding > 1f) accuracy = Mathf.Ceil(accuracy * accuracyRounding) / accuracyRounding;
        return accuracy;
    }

    public float GetNoteScore(NotePerformance notePerformance, bool allowNegativePoints = false)
    {
        float points = GetNoteAccuracy(notePerformance) * notePerformance.CorrectTime * scoringRate;
        return allowNegativePoints ? points : Mathf.Abs(points);
    }

    public LevelScoreInfo GetLevelScore()
    {
        LevelScoreInfo scoreInfo = new LevelScoreInfo();
        scoreInfo.baseScore = Mathf.FloorToInt(score);
        int comboCounter = 0;
        float totalNoteTime = 0f;
        float accuracyTime = 0f;
        foreach(NotePerformance np in performanceDetail)
        {
            // Count notes
            scoreInfo.totalNoteCount++;
            // Add note time and accuracy
            float noteTime = np.TotalPerformanceTime;
            totalNoteTime += noteTime;
            float correctTime = np.CorrectTime;
            accuracyTime += correctTime * GetNoteAccuracy(np);
            // Find correct notes
            if (correctTime > 0f)
            {
                scoreInfo.correctNoteCount++;
            }
            if (np.MissedTime == 0f && np.WrongTime == 0f)
            {
                comboCounter++;
                // Find best combo
                scoreInfo.bestCombo = Mathf.Max(scoreInfo.bestCombo, comboCounter);
            }
            else
                comboCounter = 0;
            
        }
        if (totalNoteTime > 0f)
            scoreInfo.accuracyAverage = accuracyTime / totalNoteTime;
        else
            scoreInfo.accuracyAverage = 1f;

        return scoreInfo;
    }

    private void OnPlayCorrectNote(NoteSpawn note)
    {
        if (note != null) onCorrectNote.Invoke(note, GetNoteAccuracy(note.performance), GetNoteScore(note.performance));
    }

    private void OnPlayWrongNote(NoteSpawn note)
    {
        if (note != null) onWrongNote.Invoke(note);
    }

    private void OnMissNote(NoteSpawn note)
    {
        if (note != null) onMissNote.Invoke(note);
    }

    private void OnNoteExit(NoteSpawn note)
    {
        if (note != null) AddNotePerformance(note);
    }

    private void OnNoteCrash(float deltaTime)
    {
        TakeDamage(deltaTime * damageRate);
    }
}
