using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PerformanceGUI : GameUI
{
    [Header("UI Components")]
    public CounterDisplay scoreDisplay;
    public Slider healthBar;
    public TintFlash frameTint;
    public MultiplierDisplay comboDisplay;
    public MultiplierDisplay accuracyDisplay;
    public SwooshDisplay smallMessageDisplay;
    public TransformShaker smallMessageShake;
    [Header("Content")]
    public string[] wrongNoteMessages;
    public Color wrongNoteMessageColor = Color.red;
    public float healthBarScale = 64f;

    private PerformanceJudge judge;
    private string wrongNoteMessage;

    public override Component[] UIComponents => new Component[]
        { scoreDisplay, comboDisplay, accuracyDisplay, healthBar, smallMessageDisplay };


    public PerformanceJudge Judge
    {
        get => judge;
        set
        {
            RemoveListenners(judge);
            judge = value;
            AddListenners(judge);
            GUIActive = judge;
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
        if (frameTint)
        {
            if (healthChange < 0f) frameTint.Tint();
        }
    }

    public void DisplayScore(float score)
    {
        if (scoreDisplay) scoreDisplay.value = Mathf.FloorToInt(score);
    }

    public void DisplayCorrectlyPlayedNote(NoteInstance note, float accuracy, float points)
    {
        DisplayNoteAccuracy(accuracy);
        if (note) DisplayNotePoints(points, note.DisplayColor);
    }

    public void DisplayWronglyPlayedNote(NoteInstance note)
    {
        DisplayNoteAccuracy(0f);
        DisplayMissedMessage();
    }

    public void DisplayPlayedNoteEnd(NoteInstance note, float points, int combo)
    {
        if (note) DisplayNotePointsEnd(points, note.DisplayColor);
        else DisplayNotePointsEnd();
        wrongNoteMessage = null;
        DisplayNoteCombo(combo);
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

    private void AddListenners(PerformanceJudge perfJudge)
    {
        if (perfJudge)
        {
            perfJudge.onScore.AddListener(DisplayScore);
            perfJudge.onHealth.AddListener(DisplayHealth);
            perfJudge.onCorrectNote.AddListener(DisplayCorrectlyPlayedNote);
            perfJudge.onWrongNote.AddListener(DisplayWronglyPlayedNote);
            perfJudge.onNotePerformanceEnd.AddListener(DisplayPlayedNoteEnd);
        }
    }

    private void RemoveListenners(PerformanceJudge perfJudge)
    {
        if (perfJudge)
        {
            perfJudge.onScore.RemoveListener(DisplayScore);
            perfJudge.onHealth.RemoveListener(DisplayHealth);
            perfJudge.onCorrectNote.RemoveListener(DisplayCorrectlyPlayedNote);
            perfJudge.onWrongNote.RemoveListener(DisplayWronglyPlayedNote);
            perfJudge.onNotePerformanceEnd.RemoveListener(DisplayPlayedNoteEnd);
        }
    }
}
