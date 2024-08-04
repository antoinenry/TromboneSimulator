using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteAlways]
public class JudgeGUI : GameUI
{
    [Header("UI Components")]
    public CounterDisplay scoreDisplay;
    public MultiplierDisplay comboDisplay;
    public MultiplierDisplay accuracyDisplay;
    public Slider healthBar;
    public TintFlash frame;
    public Slider powerBar;
    public SwooshDisplay smallMessageDisplay;
    public TransformShaker smallMessageShake;
    public Transform objectiveDisplay;
    [Header("Content")]
    public string[] wrongNoteMessages;
    public Color wrongNoteMessageColor = Color.red;
    public float healthBarScale = 64f;
    public Color damageFrameTint = Color.red;
    public ObjectiveCheckPanel objectivePanelPrefab;
    public float displayObjectiveDuration = 2f;

    private PerformanceJudge performance;
    private ObjectiveJudge objectives;
    private string wrongNoteMessage;
    private Coroutine displayObjectiveCoroutine;

    public override Component[] UIComponents => new Component[]
        { scoreDisplay, comboDisplay, accuracyDisplay, healthBar, smallMessageDisplay, powerBar, objectiveDisplay };


    public PerformanceJudge Performance
    {
        get => performance;
        set
        {
            RemoveListeners(performance);
            performance = value;
            AddListeners(performance);
            GUIActive = performance != null || objectives != null;
        }
    }

    public ObjectiveJudge Objectives
    {
        get => objectives;
        set
        {
            RemoveListeners(objectives);
            objectives = value;
            AddListeners(objectives);
            GUIActive = performance != null || objectives != null;
        }
    }

    public override bool GUIActive
    {
        set
        {
            if (value != GUIActive)
            {
                StopAllCoroutines();
                displayObjectiveCoroutine = null;
                DestroyObjectivePanels();
            }
            base.GUIActive = value;
        }
    }

    private void AddListeners(PerformanceJudge perfJudge)
    {
        if (perfJudge)
        {
            perfJudge.onScore.AddListener(DisplayScore);
            perfJudge.onHealth.AddListener(DisplayHealth);
            perfJudge.onCorrectNote.AddListener(DisplayCorrectlyPlayedNote);
            perfJudge.onWrongNote.AddListener(DisplayWronglyPlayedNote);
            perfJudge.onNotePerformanceEnd.AddListener(DisplayPlayedNoteEnd);
            perfJudge.onNoteCombo.AddListener(DisplayNoteCombo);
        }
    }

    private void RemoveListeners(PerformanceJudge perfJudge)
    {
        if (perfJudge)
        {
            perfJudge.onScore.RemoveListener(DisplayScore);
            perfJudge.onHealth.RemoveListener(DisplayHealth);
            perfJudge.onCorrectNote.RemoveListener(DisplayCorrectlyPlayedNote);
            perfJudge.onWrongNote.RemoveListener(DisplayWronglyPlayedNote);
            perfJudge.onNotePerformanceEnd.RemoveListener(DisplayPlayedNoteEnd);
            perfJudge.onNoteCombo.RemoveListener(DisplayNoteCombo);
        }
    }

    private void AddListeners(ObjectiveJudge objJudge)
    {
        if (objJudge)
        {
            objJudge.onNewObjectiveComplete.AddListener(DisplayObjectivePanel);
        }
    }

    private void RemoveListeners(ObjectiveJudge objJudge)
    {
        if (objJudge)
        {
            objJudge.onNewObjectiveComplete.RemoveListener(DisplayObjectivePanel);
        }
    }

    public void ResetDisplay(float maxHealth = float.NaN)
    {
        if (float.IsNaN(maxHealth)) DisplayHealth(healthBar.maxValue, healthBar.maxValue);
        else DisplayHealth(maxHealth, maxHealth);
        DisplayScore(0f);
        DisplayNoteAccuracy(1f);
        DisplayNoteCombo(0);
        DisplayNotePointsEnd();
    }

    public void DisplayHealth(float healthValue, float maxHealth = float.NaN, float healthChange = 0f)
    {
        if (healthBar)
        {
            if (float.IsNaN(maxHealth) == false) healthBar.maxValue = maxHealth * healthBarScale;
            RectTransform rect = healthBar.GetComponent<RectTransform>();
            rect.sizeDelta = new(healthBar.maxValue, rect.sizeDelta.y);
            healthBar.value = healthValue * healthBarScale;
        }
        if (frame)
        {
            if (healthChange < 0f) frame.Tint(damageFrameTint);
        }
    }

    public void DisplayScore(float score)
    {
        if (scoreDisplay) scoreDisplay.value = Mathf.FloorToInt(score);
    }

    public void DisplayCorrectlyPlayedNote(NoteSpawn note, float accuracy, float points, int comboMultiplier)
    {
        DisplayNoteAccuracy(accuracy);
        if (note) DisplayNotePoints(Mathf.RoundToInt(points) * comboMultiplier, note.DisplayColor);
    }

    public void DisplayWronglyPlayedNote(NoteSpawn note)
    {
        DisplayNoteAccuracy(0f);
        DisplayMissedMessage();
    }

    public void DisplayPlayedNoteEnd(NoteSpawn note, float points)
    {
        if (note) DisplayNotePointsEnd(Mathf.RoundToInt(points), note.DisplayColor);
        else DisplayNotePointsEnd();
        wrongNoteMessage = null;
    }

    public void DisplayNoteAccuracy(float accuracy)
    {
        if (accuracyDisplay) accuracyDisplay.value = accuracy;
    }

    public void DisplayNoteCombo(int combo)
    {
        if (comboDisplay) comboDisplay.value = combo;
    }

    public void DisplayNotePoints(float points, Color pointsColor)
    {
        if (smallMessageDisplay)
        {
            string pointsString = Mathf.FloorToInt(points).ToString();
            smallMessageDisplay.SetTextContent(pointsString);
            smallMessageDisplay.SetTextColor(pointsColor);
        }
    }

    public void DisplayNotePointsEnd(float points, Color pointsColor)
    {
        if (smallMessageDisplay)
        {
            if (points > 0f) DisplayNotePoints(points, pointsColor);
            smallMessageDisplay.FreeText();
        }
    }

    public void DisplayNotePointsEnd()
    {
        if (smallMessageDisplay)
        {
            smallMessageDisplay.FreeText();
        }
    }

    public void DisplayMissedMessage()
    {
        if (wrongNoteMessage == null && wrongNoteMessages != null && wrongNoteMessages.Length > 0)
            wrongNoteMessage = wrongNoteMessages[Random.Range(0, wrongNoteMessages.Length)];
        if (smallMessageDisplay && wrongNoteMessages != null && wrongNoteMessages.Length > 0)
        {
            smallMessageDisplay.SetTextContent(wrongNoteMessage);
            smallMessageDisplay.SetTextColor(wrongNoteMessageColor);
        }
        if (smallMessageShake) smallMessageShake.Shake();
    }

    public void DisplayObjectivePanel(ObjectiveInfo objectiveInfo)
    {
        if (objectivePanelPrefab == null || objectiveDisplay == null) return;
        StartCoroutine(QueueDisplayObjectiveCoroutine(objectiveInfo));
    }

    public void DestroyObjectivePanels()
    {
        if (objectiveDisplay == null) return;
        ObjectiveCheckPanel[] panels = objectiveDisplay.GetComponentsInChildren<ObjectiveCheckPanel>(true);
        foreach (ObjectiveCheckPanel panel in panels) Destroy(panel.gameObject);
    }

    private IEnumerator QueueDisplayObjectiveCoroutine(ObjectiveInfo objectiveInfo)
    {
        while (displayObjectiveCoroutine != null) yield return null;
        displayObjectiveCoroutine = StartCoroutine(DisplayObjectiveCoroutine(objectiveInfo));
    }

    private IEnumerator DisplayObjectiveCoroutine(ObjectiveInfo objectiveInfo)
    {
        ObjectiveCheckPanel panel = Instantiate(objectivePanelPrefab, objectiveDisplay);
        panel.SetText(objectiveInfo.name);
        panel.PlayNewlyCheckedAnimation();
        yield return new WaitForSeconds(displayObjectiveDuration);
        panel.PlayDisappearAnimation(destroyOnAnimationEnd: true);
        displayObjectiveCoroutine = null;
    }
}