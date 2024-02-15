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

    public void DisplayHealth(float healthValue, float healthChange = 0f)
    {
        if (healthBar) healthBar.value = healthValue * healthBar.maxValue;
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
