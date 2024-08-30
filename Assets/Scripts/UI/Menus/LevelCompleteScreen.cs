using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;

[ExecuteAlways]
public class LevelCompleteScreen : MenuUI
{
    [Header("Components")]
    public ScoreLineDisplay header;
    public ScoreLineDisplay title;
    public ScoreLineDisplay baseScoreDisplay;
    public ScoreLineDisplay noteCountDisplay;
    public ScoreLineDisplay comboDisplay;
    public ScoreLineDisplay accuracyDisplay;
    public ScoreLineDisplay healthDisplay;
    public ScoreLineDisplay totalDisplay;
    public Transform objectiveChecklist;
    public Button replayButton;
    public Button nextButton;
    public Button skipButton;
    public PlayerXpPanel playerXpPanel;
    [Header("Prefabs")]
    public ObjectiveCheckPanel objectiveItemPrefab;
    [Header("Timing")]
    [Min(0f)] public float startDisplayDelay = 1f;
    [Min(0f)] public float lineDisplayDelay = .5f;
    [Min(0f)] public float objectiveDisplayDelay = .5f;
    [Min(0f)] public float endDisplayDelay = 3f;
    [Header("Values")]
    public LevelScoreInfo levelScore;
    public LevelProgress levelObjectiveProgress;
    public bool[] checklist;
    public int playerXp;
    [Header("Events")]
    public UnityEvent onPressReplay;
    public UnityEvent onPressNext;
    public UnityEvent<int> onTotalScore;

    private Coroutine displayLinesCoroutine;
    private Coroutine displayObjectivesCoroutine;
    private ObjectiveCheckPanel[] objectivePanels;

    public bool DisplayCoroutine => displayLinesCoroutine != null || displayObjectivesCoroutine != null;

    #region Initialize
    override protected void Reset()
    {
        base.Reset();
    }

    protected override void Update()
    {
        base.Update();
        // Display without animation in editor mode
        if (!Application.isPlaying)
        {
            ShowAllLines();
            InstantiateObjectivePanels();
        }
        else
        {
            ToggleButtonsVisibility();
        }
        // Display player xp
        playerXpPanel?.SetXp(playerXp);
    }

    public override void ShowUI()
    {
        base.ShowUI();
        HideAllLines();
        DestroyObjectivePanels();
        if (Application.isPlaying)
        {
            AddButtonListeners();
            displayLinesCoroutine = StartCoroutine(DisplayLinesCoroutine());
            displayObjectivesCoroutine = StartCoroutine(DisplayObjectivesCoroutine());
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        HideAllLines();
        DestroyObjectivePanels();
        if (Application.isPlaying)
        {
            RemoveButtonListeners();
            if (displayLinesCoroutine != null) StopCoroutine(displayLinesCoroutine);
            displayLinesCoroutine = null;
            if (displayObjectivesCoroutine != null) StopCoroutine(displayObjectivesCoroutine);
        }
    }
    #endregion

    #region Lines (header and score)
    public void ShowAllLines()
    {
        ShowLine(header);
        ShowLine(title);
        SetLineTexts(title, levelObjectiveProgress.levelAsset?.levelName);

        ShowLine(baseScoreDisplay);
        SetLineValuesImmediate(baseScoreDisplay, levelScore.baseScore);

        ShowLine(noteCountDisplay);
        SetLineValuesImmediate(noteCountDisplay, levelScore.correctNoteCount, levelScore.totalNoteCount, levelScore.PlayedNoteBonus);

        ShowLine(comboDisplay);
        SetLineValuesImmediate(comboDisplay, levelScore.bestCombo, levelScore.ComboBonus);

        ShowLine(accuracyDisplay);
        SetLineValuesImmediate(accuracyDisplay, levelScore.accuracyAverage, levelScore.AccuracyBonus);

        ShowLine(healthDisplay);
        SetLineValuesImmediate(healthDisplay, levelScore.remainingHealth, levelScore.HealthBonus);

        ShowLine(totalDisplay);
        SetLineValuesImmediate(totalDisplay, levelScore.Total);
    }

    public void HideAllLines()
    {
        HideLine(header);
        HideLine(title);
        HideLine(baseScoreDisplay);
        HideLine(noteCountDisplay);
        HideLine(accuracyDisplay);
        HideLine(healthDisplay);
        HideLine(comboDisplay);
        HideLine(totalDisplay);
    }

    private IEnumerator DisplayLinesCoroutine()
    {
        yield return new WaitForSeconds(startDisplayDelay);
        // Display lines one at a time
        float displayTimer = 0f;
        yield return new WaitForSeconds(lineDisplayDelay);

        // Header
        ShowLine(header);
        yield return new WaitForSeconds(lineDisplayDelay);
        // Level title
        SetLineTexts(title, levelObjectiveProgress.levelAsset?.levelName);
        ShowLine(title);
        yield return new WaitForSeconds(lineDisplayDelay);

        // Base score
        SetLineValues(baseScoreDisplay, levelScore.baseScore);
        ShowLine(baseScoreDisplay);

        // Total score (delayed, start at zero points)
        LevelScoreInfo delayedScore = levelScore;
        delayedScore.ClearPerformance();
        SetLineValues(totalDisplay, 0);
        ShowLine(totalDisplay);
        displayTimer = lineDisplayDelay;
        do
        {
            delayedScore.baseScore = (int)baseScoreDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || baseScoreDisplay.IsMoving);
        onTotalScore.Invoke(delayedScore.Total);

        // Note count
        SetLineValues(noteCountDisplay, levelScore.correctNoteCount, levelScore.totalNoteCount, levelScore.PlayedNoteBonus);
        ShowLine(noteCountDisplay);
        displayTimer = lineDisplayDelay;
        do
        {
            delayedScore.correctNoteCount = (int)noteCountDisplay.valueDisplays[0].CurrentDisplayValue;
            delayedScore.totalNoteCount = (int)noteCountDisplay.valueDisplays[1].CurrentDisplayValue - delayedScore.correctNoteCount;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || noteCountDisplay.IsMoving);
        onTotalScore.Invoke(delayedScore.Total);

        // Combo
        SetLineValues(comboDisplay, levelScore.bestCombo, levelScore.ComboBonus);
        ShowLine(comboDisplay);
        displayTimer = lineDisplayDelay;
        do
        {
            delayedScore.bestCombo = (int)comboDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || comboDisplay.IsMoving);
        onTotalScore.Invoke(delayedScore.Total);

        // Accuracy
        SetLineValues(accuracyDisplay, levelScore.accuracyAverage, levelScore.AccuracyBonus);
        ShowLine(accuracyDisplay);
        displayTimer = lineDisplayDelay;
        do
        {
            delayedScore.accuracyAverage = accuracyDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || accuracyDisplay.IsMoving);
        onTotalScore.Invoke(delayedScore.Total);

        // Health
        SetLineValues(healthDisplay, levelScore.remainingHealth, levelScore.HealthBonus);
        ShowLine(healthDisplay);
        displayTimer = lineDisplayDelay;
        do
        {
            delayedScore.remainingHealth = healthDisplay.valueDisplays[0].CurrentDisplayValue;
            SetLineValues(totalDisplay, delayedScore.Total);
            yield return null;
            displayTimer -= Time.deltaTime;
        }
        while (displayTimer > 0f || healthDisplay.IsMoving);
        onTotalScore.Invoke(delayedScore.Total);

        // End
        yield return new WaitWhile(() => totalDisplay.IsMoving);
        yield return new WaitForSeconds(endDisplayDelay);
        displayLinesCoroutine = null;
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

    private void SetLineValuesImmediate(ScoreLineDisplay line, params float[] values)
    {
        if (line != null) line.SetValuesImmediate(values);
    }
    #endregion

    #region Objectives
    public void InstantiateObjectivePanels()
    {
        if (objectiveChecklist == null)
        {
            objectivePanels = null;
            return;
        }
        int objectiveCount = levelObjectiveProgress.ObjectiveCount;
        // Correct number of panels
        objectivePanels = objectiveChecklist.GetComponentsInChildren<ObjectiveCheckPanel>(true);
        int panelCount = objectivePanels != null ? objectivePanels.Length : 0;
        if (panelCount != objectiveCount)
        {
            DestroyObjectivePanels();
            objectivePanels = new ObjectiveCheckPanel[objectiveCount];
            for (int i = 0; i < objectiveCount; i++) objectivePanels[i] = Instantiate(objectiveItemPrefab, objectiveChecklist);
            panelCount = objectiveCount;
        }
        // Set panel looks
        string[] objectiveNames = levelObjectiveProgress.ObjectiveNames;
        for (int i = 0; i < objectiveCount; i++)
        {
            ObjectiveCheckPanel o = objectivePanels[i];
            o.SetText(objectiveNames[i]);
        }
    }

    public void DestroyObjectivePanels()
    {
        if (objectiveChecklist == null) return;
        ObjectiveCheckPanel[] objectivePanels = objectiveChecklist.GetComponentsInChildren<ObjectiveCheckPanel>(true);
        foreach (ObjectiveCheckPanel panel in objectivePanels) DestroyImmediate(panel.gameObject);
    }

    private IEnumerator DisplayObjectivesCoroutine()
    {
        yield return new WaitForSeconds(startDisplayDelay);
        // Instantiate and hide objectives
        InstantiateObjectivePanels();
        if (objectivePanels == null) yield break;
        foreach (ObjectiveCheckPanel panel in objectivePanels) panel.gameObject.SetActive(false);
        // Animate objectives
        int objectiveCount = levelObjectiveProgress.ObjectiveCount;
        bool[] oldChecklist = levelObjectiveProgress.Checklist;
        int levelCheckCount = oldChecklist != null ? oldChecklist.Length : 0;
        int checkCount = checklist != null ? checklist.Length : 0;
        for (int i = 0; i < objectiveCount; i++)
        {
            yield return new WaitForSeconds(objectiveDisplayDelay);
            ObjectiveCheckPanel o = objectivePanels[i];
            if (o == null) continue;
            o.gameObject.SetActive(true);
            if (i < levelCheckCount && oldChecklist[i] == true)
                o.PlayAlreadyCheckedAnimation();
            else if (i < checkCount && checklist[i])
            {
                o.PlayNewlyCheckedAnimation();
                playerXp += 1;
            }
            else
                o.PlayUncheckedAnimation();
        }
        yield return new WaitForSeconds(endDisplayDelay);
        displayObjectivesCoroutine = null;
    }

    public bool TryCheckObjective(ObjectiveInfo objective)
    {
        int checkCount = checklist != null ? checklist.Length : 0;
        int objectiveIndex = levelObjectiveProgress.GetObjectiveIndex(objective);
        if (objectiveIndex < 0 || objectiveIndex > checkCount) return false;
        // Update checklist
        checklist[objectiveIndex] = true;
        // Play animation if needed
        ObjectiveCheckPanel matchingPanel = objectivePanels != null ? Array.Find(objectivePanels, p => p.GetText() == objective.name) : null;
        if (matchingPanel != null && matchingPanel.gameObject.activeInHierarchy) matchingPanel.PlayLateCheckAnimation();
        // Success
        return true;
    }
    #endregion

    #region Buttons

    private void AddButtonListeners()
    {
        skipButton?.onClick?.AddListener(OnPressSkip);
        replayButton?.onClick?.AddListener(OnPressReplay);
        nextButton?.onClick?.AddListener(OnPressNext);
    }

    private void RemoveButtonListeners()
    {
        skipButton?.onClick?.RemoveListener(OnPressSkip);
        replayButton?.onClick?.RemoveListener(OnPressReplay);
        nextButton?.onClick?.RemoveListener(OnPressNext);
    }

    private void ToggleButtonsVisibility()
    {
        // Display buttons depending on coroutine state
        if (DisplayCoroutine)
        {
            skipButton?.gameObject?.SetActive(true);
            replayButton?.gameObject?.SetActive(false);
            nextButton?.gameObject?.SetActive(false);
        }
        else
        {
            skipButton?.gameObject?.SetActive(false);
            replayButton?.gameObject?.SetActive(true);
            nextButton?.gameObject?.SetActive(true);
        }
    }

    private void OnPressSkip()
    {
        if (displayLinesCoroutine != null)
        {
            StopCoroutine(displayLinesCoroutine);
            displayLinesCoroutine = null;
        }
        ShowAllLines();
        onTotalScore.Invoke(levelScore.Total);
    }

    private void OnPressNext()
    {
        onPressNext.Invoke();
        HideUI();
    }

    private void OnPressReplay()
    {
        onPressReplay.Invoke();
        HideUI();
    }
    #endregion
}
