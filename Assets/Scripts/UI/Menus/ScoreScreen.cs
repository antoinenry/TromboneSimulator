using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
public class ScoreScreen : MenuUI
{
    [Header("Level display")]
    public ScoreLineDisplay header;
    public ScoreLineDisplay title;
    [Header("Score display")]
    public ScoreLineDisplay baseScoreDisplay;
    public ScoreLineDisplay noteCountDisplay;
    public ScoreLineDisplay accuracyDisplay;
    public ScoreLineDisplay comboDisplay;
    public ScoreLineDisplay totalDisplay;
    [Header("Display Parameters")]
    public Button nextButton;
    [Header("Timing")]
    [Min(0f)] public float displayDelay = .5f;
    [Min(0f)] public float endDisplayDelay = 3f;
    [Header("Values")]
    public string levelName;
    public LevelScoreInfo score;
    public int levelIndex;

    private Coroutine displayLinesCoroutine;

    override protected void Reset()
    {
        base.Reset();
        // Try to find components
        ScoreLineDisplay[] getLines = GetComponentsInChildren<ScoreLineDisplay>(true);
        int lCount = getLines.Length;
        if (lCount > 0) header = getLines[0];
        if (lCount > 1) title = getLines[1];
        if (lCount > 2) baseScoreDisplay = getLines[2];
        if (lCount > 3) noteCountDisplay = getLines[3];
        if (lCount > 4) accuracyDisplay = getLines[4];
        if (lCount > 5) comboDisplay = getLines[5];
        if (lCount > 6) totalDisplay = getLines[6];
    }

    protected override void Awake()
    {
        base.Awake();
        UIScore = this;
    }

    protected override void Update()
    {
        base.Update();
        // Display without animation in editor mode
        if (!Application.isPlaying) ShowAllLines();
    }

    public override void ShowUI()
    {
        base.ShowUI();
        HideAllLines();
        if (Application.isPlaying)
        {
            nextButton.onClick.AddListener(OnPressNext);
            displayLinesCoroutine = StartCoroutine(DisplayLinesCoroutine());
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        HideAllLines();
        if (Application.isPlaying)
        {
            nextButton.onClick.RemoveListener(OnPressNext);
            if (displayLinesCoroutine != null) StopCoroutine(displayLinesCoroutine);
            displayLinesCoroutine = null;
        }
    }

    public void ShowAllLines()
    {
        ShowLine(header);
        ShowLine(title);
        SetLineTexts(title, levelName);
        ShowLine(baseScoreDisplay);
        SetLineValues(baseScoreDisplay, score.baseScore);
        ShowLine(accuracyDisplay);
        SetLineValues(accuracyDisplay, score.accuracyAverage, score.AccuracyBonus);
        ShowLine(comboDisplay);
        SetLineValues(comboDisplay, score.bestCombo, score.ComboBonus);
        ShowLine(noteCountDisplay);
        SetLineValues(noteCountDisplay, score.correctNoteCount, score.totalNoteCount, score.PlayedNoteBonus);
        ShowLine(totalDisplay);
        SetLineValues(totalDisplay, score.Total);
    }

    public void HideAllLines()
    {
        HideLine(header);
        HideLine(title);
        HideLine(baseScoreDisplay);
        HideLine(noteCountDisplay);
        HideLine(accuracyDisplay);
        HideLine(comboDisplay);
        HideLine(totalDisplay);
    }

    private IEnumerator DisplayLinesCoroutine()
    {
        // Display lines one at a time
        float displayTimer = 0f;
        yield return new WaitForSeconds(displayDelay);
        // Header
        ShowLine(header);
        yield return new WaitForSeconds(displayDelay);
        // Level title
        SetLineTexts(title, levelName);
        ShowLine(title);
        yield return new WaitForSeconds(displayDelay);
        // Base score
        SetLineValues(baseScoreDisplay, score.baseScore);
        ShowLine(baseScoreDisplay);
        // Total score (delayed)
        LevelScoreInfo delayedScore = LevelScoreInfo.Zero;
        SetLineValues(totalDisplay, 0);
        ShowLine(totalDisplay);
        displayTimer = displayDelay;
        do
        {
            delayedScore.baseScore = (int)baseScoreDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || baseScoreDisplay.IsMoving || totalDisplay.IsMoving);    
        // Accuracy
        SetLineValues(accuracyDisplay, score.accuracyAverage, score.AccuracyBonus);
        ShowLine(accuracyDisplay);
        displayTimer = displayDelay;
        do
        {
            delayedScore.accuracyAverage = accuracyDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || accuracyDisplay.IsMoving || totalDisplay.IsMoving);
        // Combo
        SetLineValues(comboDisplay, score.bestCombo, score.ComboBonus);
        ShowLine(comboDisplay);
        displayTimer = displayDelay;
        do
        {
            delayedScore.bestCombo = (int)comboDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || comboDisplay.IsMoving || totalDisplay.IsMoving);
        // Note count
        SetLineValues(noteCountDisplay, score.correctNoteCount, score.totalNoteCount, score.PlayedNoteBonus);
        ShowLine(noteCountDisplay);
        displayTimer = displayDelay;
        do
        {
            delayedScore.correctNoteCount = (int)noteCountDisplay.valueDisplays[0].CurrentDisplayValue;
            delayedScore.totalNoteCount = (int)noteCountDisplay.valueDisplays[1].CurrentDisplayValue - delayedScore.correctNoteCount;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || noteCountDisplay.IsMoving || totalDisplay.IsMoving);
    }

    public void DisplayScore(int lvlIndex, string lvlName, LevelScoreInfo scr)
    {
        levelIndex = lvlIndex;
        levelName = lvlName;
        score = scr;
        ShowUI();
    }

    private void ShowLine(ScoreLineDisplay line)
    {
        if (line != null)
        {
            line.visible = true;
            line.Update();
        }
    }

    private void HideLine(ScoreLineDisplay line)
    {
        if (line != null)
        {
            line.ResetValues();
            line.visible = false;
            line.Update();
        }
    }

    private void SetLineTexts(ScoreLineDisplay line, params string[] texts)
    {
        if (line != null) line.SetTexts(texts);
    }

    private void SetLineValues(ScoreLineDisplay line, params float[] values)
    {
        if (line != null) line.SetValues(values);
    }

    private void OnPressNext()
    {
        if (displayLinesCoroutine != null)
        {
            StopCoroutine(displayLinesCoroutine);
            displayLinesCoroutine = null;
            ShowAllLines();
        }
        else
        {
            HideUI();
        }
    }
}
