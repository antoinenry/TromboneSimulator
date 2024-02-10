using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class PerformanceGUI : GameUI
{
    [Header("UI Components")]
    public CounterDisplay scoreDisplay;
    public Slider healthBar;
    public MultiplierDisplay comboDisplay;
    public MultiplierDisplay accuracyDisplay;
    public SwooshDisplay onInstrumentDisplay;
    [Header("Content")]
    public string[] missedMessages;
    public Color missedMessageColor = Color.red;
    
    private string randomMissedMessage;

    public override Component[] UIComponents => new Component[]
        { scoreDisplay, comboDisplay, accuracyDisplay, healthBar, onInstrumentDisplay };

    public void SetHealthBar(float hp)
    {
        if (healthBar) healthBar.value = hp * healthBar.maxValue;
    }


    public void SetScore(float score)
    {
        if (scoreDisplay) scoreDisplay.value = Mathf.FloorToInt(score);
    }

    public void SetNoteAccuracy(float accuracy)
    {
        if (accuracyDisplay) accuracyDisplay.value = accuracy;
    }

    public void SetNoteCombo(int combo)
    {
        if (comboDisplay) comboDisplay.value = combo;
    }

    public void SetNotePoints(float points, Color pointsColor)
    {
        if (onInstrumentDisplay)
        {
            string pointsString = Mathf.FloorToInt(points).ToString();
            onInstrumentDisplay.SetTextContent(pointsString);
            onInstrumentDisplay.SetTextColor(pointsColor);
        }
    }

    public void EndNotePoints(float points, Color pointsColor)
    {
        if (onInstrumentDisplay)
        {
            if (points > 0f) SetNotePoints(points, pointsColor);
            onInstrumentDisplay.FreeText();
            if (missedMessages != null && missedMessages.Length > 0)
                randomMissedMessage = missedMessages[Random.Range(0, missedMessages.Length)];
        }
    }

    public void EndNotePoints()
    {
        if (onInstrumentDisplay)
        {
            onInstrumentDisplay.FreeText();
        }
    }

    public void ShowMissedMessage()
    {
        if (onInstrumentDisplay && missedMessages != null && missedMessages.Length > 0)
        {
            onInstrumentDisplay.SetTextContent(randomMissedMessage);
            onInstrumentDisplay.SetTextColor(missedMessageColor);
        }
    }

}
