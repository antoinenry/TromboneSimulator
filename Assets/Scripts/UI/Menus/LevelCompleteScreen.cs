using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[ExecuteAlways]
public class LevelCompleteScreen : MenuUI
{
    [Header("Components")]
    public ScoreLineDisplay header;
    public ScoreLineDisplay title;
    public ScoreLineDisplay baseScoreDisplay;
    public ScoreLineDisplay noteCountDisplay;
    public ScoreLineDisplay accuracyDisplay;
    public ScoreLineDisplay comboDisplay;
    public ScoreLineDisplay totalDisplay;
    public Transform objectiveChecklist;
    public Button replayButton;
    public Button nextButton;
    public Button skipButton;
    [Header("Prefabs")]
    public ObjectiveCheckPanel objectiveItemPrefab;
    [Header("Timing")]
    [Min(0f)] public float lineDisplayDelay = .5f;
    [Min(0f)] public float objectiveDisplayDelay = .5f;
    [Min(0f)] public float endDisplayDelay = 3f;
    [Header("Values")]
    public LevelScoreInfo levelScore;
    public LevelProgress levelProgress;
    public bool[] checklist;
    [Header("Events")]
    public UnityEvent onPressReplay;
    public UnityEvent onPressNext;

    private Coroutine displayLinesCoroutine;
    private Coroutine displayObjectivesCoroutine;

    public bool DisplayCoroutine => displayLinesCoroutine != null || displayObjectivesCoroutine != null; 

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

    #region Lines (header and score)
    public void ShowAllLines()
    {
        ShowLine(header);
        ShowLine(title);
        SetLineTexts(title, levelProgress.levelAsset?.levelName);
        ShowLine(baseScoreDisplay);
        SetLineValuesImmediate(baseScoreDisplay, levelScore.baseScore);
        ShowLine(accuracyDisplay);
        SetLineValuesImmediate(accuracyDisplay, levelScore.accuracyAverage, levelScore.AccuracyBonus);
        ShowLine(comboDisplay);
        SetLineValuesImmediate(comboDisplay, levelScore.bestCombo, levelScore.ComboBonus);
        ShowLine(noteCountDisplay);
        SetLineValuesImmediate(noteCountDisplay, levelScore.correctNoteCount, levelScore.totalNoteCount, levelScore.PlayedNoteBonus);
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
        HideLine(comboDisplay);
        HideLine(totalDisplay);
    }

    private IEnumerator DisplayLinesCoroutine()
    {
        // Display lines one at a time
        float displayTimer = 0f;
        yield return new WaitForSeconds(lineDisplayDelay);
        // Header
        ShowLine(header);
        yield return new WaitForSeconds(lineDisplayDelay);
        // Level title
        SetLineTexts(title, levelProgress.levelAsset?.levelName);
        ShowLine(title);
        yield return new WaitForSeconds(lineDisplayDelay);
        // Base score
        SetLineValues(baseScoreDisplay, levelScore.baseScore);
        ShowLine(baseScoreDisplay);
        // Total score (delayed)
        LevelScoreInfo delayedScore = LevelScoreInfo.Zero;
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
        while (displayTimer > 0f || baseScoreDisplay.IsMoving || totalDisplay.IsMoving);    
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
        while (displayTimer > 0f || accuracyDisplay.IsMoving || totalDisplay.IsMoving);
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
        while (displayTimer > 0f || comboDisplay.IsMoving || totalDisplay.IsMoving);
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
        while (displayTimer > 0f || noteCountDisplay.IsMoving || totalDisplay.IsMoving);
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
    public ObjectiveCheckPanel[] InstantiateObjectivePanels()
    {
        if (objectiveChecklist == null) return null;
        int objectiveCount = levelProgress.ObjectiveCount;
        // Correct number of panels
        ObjectiveCheckPanel[] objectivePanels = objectiveChecklist.GetComponentsInChildren<ObjectiveCheckPanel>(true);
        int panelCount = objectivePanels != null ? objectivePanels.Length : 0;
        if (panelCount != objectiveCount)
        {
            DestroyObjectivePanels();
            objectivePanels = new ObjectiveCheckPanel[objectiveCount];
            for (int i = 0; i < objectiveCount; i++) objectivePanels[i] = Instantiate(objectiveItemPrefab, objectiveChecklist);
            panelCount = objectiveCount;
        }
        // Set panel looks
        string[] objectiveNames = levelProgress.ObjectiveNames;
        for (int i = 0; i < objectiveCount; i++)
        {
            ObjectiveCheckPanel o = objectivePanels[i];
            o.SetText(objectiveNames[i]);
        }
        // Return instances
        return objectivePanels;
    }

    public void DestroyObjectivePanels()
    {
        if (objectiveChecklist == null) return;
        ObjectiveCheckPanel[] objectivePanels = objectiveChecklist.GetComponentsInChildren<ObjectiveCheckPanel>(true);
        foreach (ObjectiveCheckPanel panel in objectivePanels) DestroyImmediate(panel.gameObject);
    }

    private IEnumerator DisplayObjectivesCoroutine()
    {
        // Inatantiate and hide objectives
        ObjectiveCheckPanel[] objectivePanels = InstantiateObjectivePanels();
        if (objectivePanels == null) yield break;
        foreach (ObjectiveCheckPanel panel in objectivePanels) panel.gameObject.SetActive(false);
        // Animate objectives
        int objectiveCount = levelProgress.ObjectiveCount;
        bool[] levelChecklist = levelProgress.checkList;
        int levelCheckCount = levelChecklist != null ? levelChecklist.Length : 0;
        int checkCount = checklist != null ? checklist.Length : 0;
        for (int i = 0; i < objectiveCount; i++)
        {
            yield return new WaitForSeconds(objectiveDisplayDelay);
            ObjectiveCheckPanel o = objectivePanels[i];
            if (o == null) continue;
            o.gameObject.SetActive(true);
            if (i < levelCheckCount && levelChecklist[i] == true) o.PlayAlreadyCheckedAnimation();
            else if (i < checkCount && checklist[i]) o.PlayNewlyCheckedAnimation();
            else o.PlayUncheckedAnimation();
        }
        displayObjectivesCoroutine = null;
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
