using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PerformanceGUI : GameUI
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
    [Header("Content")]
    public string[] wrongNoteMessages;
    public Color wrongNoteMessageColor = Color.red;
    public float healthBarScale = 64f;
    public Color damageFrameTint = Color.red;

    private PerformanceJudge judge;
    private string wrongNoteMessage;

    public override Component[] UIComponents => new Component[]
        { scoreDisplay, comboDisplay, accuracyDisplay, healthBar, smallMessageDisplay, powerBar };


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

    private void AddListenners(PerformanceJudge perfJudge)
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

    private void RemoveListenners(PerformanceJudge perfJudge)
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

    public void DisplayCorrectlyPlayedNote(NoteSpawn note, float accuracy, float points)
    {
        DisplayNoteAccuracy(accuracy);
        if (note) DisplayNotePoints(points, note.DisplayColor);
    }

    public void DisplayWronglyPlayedNote(NoteSpawn note)
    {
        DisplayNoteAccuracy(0f);
        DisplayMissedMessage();
    }

    public void DisplayPlayedNoteEnd(NoteSpawn note, float points)
    {
        if (note) DisplayNotePointsEnd(points, note.DisplayColor);
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
}