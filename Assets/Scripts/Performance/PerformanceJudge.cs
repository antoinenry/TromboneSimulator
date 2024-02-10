using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PerformanceJudge : MonoBehaviour
{
    [Header("Difficulty")]
    public float scoringRate = 10f;
    public float damageRate = .2f;
    public float accuracyRounding = 10f;
    public int danceLength = 16;
    public int danceBuffer = 2;
    [Header("Performance")]
    public List<NotePerformance> performanceDetail;
    public float score;
    public int combo;
    public float health;
    public int dance;
    [Header("Events")]
    public UnityEvent<float> onScore;
    public UnityEvent<float> onHealth;
    public UnityEvent<int> onDance;
    public UnityEvent<NoteInstance, float, float> onCorrectNote;
    public UnityEvent<NoteInstance> onWrongNote;
    public UnityEvent<NoteInstance> onMissNote;
    public UnityEvent<NoteInstance, float, bool> onNotePerformanceEnd;

    private NoteCatcher noteCatcher;
    private NoteCrasher noteCrasher;
    private DanceDetector danceDetector;
    private int danceBufferCounter;

    private void Awake()
    {
        noteCatcher = FindObjectOfType<NoteCatcher>(true);
        noteCrasher = FindObjectOfType<NoteCrasher>(true);
        danceDetector = FindObjectOfType<DanceDetector>(true);
    }

    private void OnEnable()
    {
        if (noteCatcher != null)
        {
            noteCatcher.onCorrectNote.AddListener(OnPlayCorrectNote);
            noteCatcher.onWrongNote.AddListener(OnPlayWrongNote);
            noteCatcher.onFullCorrectNote.AddListener(OnPlayFullNote);
            noteCatcher.onMissNote.AddListener(OnMissNote);
            noteCatcher.onNoteCatchEnd.AddListener(OnNoteExit);
        }
        if (noteCrasher != null)
        {
            noteCrasher.onHorizontalCrash.AddListener(OnNoteCrash);
        }
        if (danceDetector != null)
        {
            danceDetector.onDanceBeat.AddListener(OnDanceBeat);
            danceDetector.onMissBeat.AddListener(OnMissDanceBeat);
        }
    }

    private void OnDisable()
    {
        if (noteCatcher != null)
        {
            noteCatcher.onCorrectNote.RemoveListener(OnPlayCorrectNote);
            noteCatcher.onWrongNote.RemoveListener(OnPlayWrongNote);
            noteCatcher.onFullCorrectNote.RemoveListener(OnPlayFullNote);
            noteCatcher.onMissNote.RemoveListener(OnMissNote);
            noteCatcher.onNoteCatchEnd.RemoveListener(OnNoteExit);
        }
        if (noteCrasher != null)
        {
            noteCrasher.onHorizontalCrash.RemoveListener(OnNoteCrash);
        }
        if (danceDetector != null)
        {
            danceDetector.onDanceBeat.RemoveListener(OnDanceBeat);
            danceDetector.onMissBeat.RemoveListener(OnMissDanceBeat);
        }
    }

    public void ResetPerformance()
    {
        // Reset values
        performanceDetail = new List<NotePerformance>();
        score = 0f;
        onScore.Invoke(score);
        health = 1f;
        onHealth.Invoke(health);
        dance = 0;
        onDance.Invoke(dance);
        combo = 0;
        onNotePerformanceEnd.Invoke(null, 0f, false);
    }

    public void LevelSetup(SheetMusic lvl, string playedInstrument)
    {
        // Reset
        ResetPerformance();
        // Prepare level
        if (lvl != null) performanceDetail = new List<NotePerformance>(lvl.GetPartLength(playedInstrument));
    }       

    private void OnPlayCorrectNote(NoteInstance note)
    {
        if (note != null) onCorrectNote.Invoke(note, GetNoteAccuracy(note.performance), GetNoteScore(note.performance));
    }

    private void OnPlayFullNote(NoteInstance note)
    {
        if (note != null) AddNotePerformance(note);
    }

    private void OnPlayWrongNote(NoteInstance note)
    {
        if (note != null) onWrongNote.Invoke(note);
    }

    private void OnMissNote(NoteInstance note)
    {
        if (note != null) onMissNote.Invoke(note);
    }

    private void OnNoteExit(NoteInstance note)
    {
        if (note != null)
        {
            // If note was fully caught, performance has already been added by OnPlayFullNote
            if (note.catchState == NoteInstance.CatchState.All) return;
            else AddNotePerformance(note);
        }
    }

    private void OnNoteCrash(float deltaTime)
    {
        TakeDamage(deltaTime * damageRate);
    }

    private void OnDanceBeat()
    {
        if (dance == 0 && danceBufferCounter < danceBuffer) danceBufferCounter++;
        else
        {
            dance = Mathf.Clamp(dance + 1, 0, danceLength);
            onDance.Invoke(dance);
        }
    }

    private void OnMissDanceBeat()
    {
        if (danceBufferCounter > 0) danceBufferCounter--;
        else
        {
            dance = Mathf.Clamp(dance - 1, 0, danceLength);
            onDance.Invoke(dance);
        }
    }

    public void AddNotePerformance(NoteInstance instance)
    {
        // Add note performance
        performanceDetail.Add(instance.performance);
        // Add points to score
        float noteScore = GetNoteScore(instance.performance);
        score += noteScore * combo;
        onScore.Invoke(score);
        // Increase combo if note was fully played
        bool fullPlay = instance.performance.CorrectTime == instance.Duration;
        if (fullPlay) combo++;
        // Else break combo
        else combo = 0;
        onNotePerformanceEnd.Invoke(instance, noteScore, fullPlay);
    }

    public void TakeDamage(float damagePoints)
    {
        health = Mathf.Max(health - damagePoints, 0f);
        onHealth.Invoke(health);
    }

    public void HealDamage(float healPoints)
    {
        health = Mathf.Min(health + healPoints, 1f);
        onHealth.Invoke(health);
    }

    public float GetNoteAccuracy(NotePerformance notePerformance, bool rounded = true)
    {
        if (notePerformance.CorrectTime == 0f) return 0f;
        //float errorPerSecond = NotePerformance.PerformanceSegment.ToneErrorAverage(notePerformance.CorrectSegments) / notePerformance.TotalPerformanceTime;
        //float accuracy = noteCatcher.toneTolerance != 0f ? 1f - errorPerSecond / noteCatcher.toneTolerance : 1f;
        float averageError = NotePerformance.PerformanceSegment.ToneErrorAverage(notePerformance.CorrectSegments);
        float accuracy = noteCatcher.toneTolerance != 0f ? 1f - averageError / noteCatcher.toneTolerance : 1f;
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
}
