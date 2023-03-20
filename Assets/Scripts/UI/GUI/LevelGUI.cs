using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelGUI : GameUI
{
    [Header("UI Components")]
    public Button pauseButton;
    public CounterDisplay scoreDisplay;
    public MultiplierDisplay comboDisplay;
    public MultiplierDisplay accuracyDisplay;
    public Slider timeBar;
    public Slider healthBar;
    public Slider danceBar;
    public Button powerButton;
    public SwooshDisplay pointsDisplay;
    public SwooshDisplay messageDisplay;
    [Header("Content")]
    public string grabTromboneMessage = "a vos coulisses";
    public int startCountDownAt = 3;
    public string[] missedMessages;
    public Color missedMessageColor = Color.red;
    [Header("Events")]
    public UnityEvent onPressPause;
    public UnityEvent onPressPower;

    private bool isShowingMissedMessage;
    private string randomMissedMessage;

    private void OnEnable()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(OnClickPauseButton);
        if (Application.isPlaying) SetPowerButton(false);
    }

    private void OnDisable()
    {
        if (pauseButton != null) pauseButton.onClick.RemoveListener(OnClickPauseButton);
    }

    public override bool GUIActive
    {
        get
        {
            if (pauseButton != null && pauseButton.gameObject.activeInHierarchy == false) return false;
            if (scoreDisplay != null && scoreDisplay.gameObject.activeInHierarchy == false) return false;
            if (comboDisplay != null && comboDisplay.gameObject.activeInHierarchy == false) return false;
            if (accuracyDisplay != null && accuracyDisplay.gameObject.activeInHierarchy == false) return false;
            if (timeBar != null && timeBar.gameObject.activeInHierarchy == false) return false;
            if (healthBar != null && healthBar.gameObject.activeInHierarchy == false) return false;
            if (danceBar != null && danceBar.gameObject.activeInHierarchy == false) return false;
            if (powerButton != null && powerButton.gameObject.activeInHierarchy == false) return false;
            if (pointsDisplay != null && pointsDisplay.gameObject.activeInHierarchy == false) return false;
            if (messageDisplay != null && messageDisplay.gameObject.activeInHierarchy == false) return false;
            return true;
        }

        set
        {
            if (pauseButton != null) pauseButton.gameObject.SetActive(value);
            if (scoreDisplay != null) scoreDisplay.gameObject.SetActive(value);
            if (comboDisplay != null) comboDisplay.gameObject.SetActive(value);
            if (accuracyDisplay != null) accuracyDisplay.gameObject.SetActive(value);
            if (timeBar != null) timeBar.gameObject.SetActive(value);
            if (healthBar != null) healthBar.gameObject.SetActive(value);
            if (danceBar != null) danceBar.gameObject.SetActive(value);
            if (powerButton != null) powerButton.gameObject.SetActive(value);
            if (pointsDisplay != null) pointsDisplay.gameObject.SetActive(value);
            if (messageDisplay != null) messageDisplay.gameObject.SetActive(value);
        }
    }

    private void OnClickPauseButton()
    {
        onPressPause.Invoke();
    }

    private void OnClickPowerButton()
    {
        onPressPower.Invoke();
        SetPowerButton(false);
    }

    public void SetTimeBar(float time, float maxTime, bool interactable = false)
    {
        if (timeBar != null)
        {
            timeBar.interactable = interactable;
            timeBar.value = (time / maxTime) * timeBar.maxValue;
        }
    }

    public void SetHealthBar(float hp)
    {
        if (healthBar != null) healthBar.value = hp * healthBar.maxValue;
    }

    public void SetDanceBar(float danceLevel)
    {
        if (danceBar != null) danceBar.value = danceLevel;
    }

    public void SetPowerButton(bool enable)
    {
        if (powerButton != null && enable != powerButton.interactable)
        {
            if (enable) powerButton.onClick.AddListener(OnClickPowerButton);
            else powerButton.onClick.RemoveListener(OnClickPowerButton);
        }
        powerButton.interactable = enable;
    }

    public void SetScore(float score)
    {
        if (scoreDisplay != null) scoreDisplay.value = Mathf.FloorToInt(score);
    }

    public void SetNoteAccuracy(float accuracy)
    {
        if (accuracyDisplay != null) accuracyDisplay.value = accuracy;
    }

    public void SetNoteCombo(int combo)
    {
        if (comboDisplay != null) comboDisplay.value = combo;
    }

    public void SetNotePoints(float points, Color pointsColor)
    {
        if (pointsDisplay != null)
        {
            if (isShowingMissedMessage)
            {
                //pointsDisplay.FreeText();
                isShowingMissedMessage = false;
            }
            string pointsString = Mathf.FloorToInt(points).ToString();
            pointsDisplay.SetTextContent(pointsString);
            pointsDisplay.SetTextColor(pointsColor);
        }
    }

    public void EndNotePoints(float points, Color pointsColor)
    {
        if (pointsDisplay != null)
        {
            if (points > 0f) SetNotePoints(points, pointsColor);
            pointsDisplay.FreeText();
            if (missedMessages != null && missedMessages.Length > 0)
                randomMissedMessage = missedMessages[Random.Range(0, missedMessages.Length)];
        }
    }

    public void EndNotePoints()
    {
        if (pointsDisplay != null)
        {
            pointsDisplay.FreeText();
        }
    }

    public void ShowMissedMessage()
    {
        if (pointsDisplay != null && missedMessages != null && missedMessages.Length > 0)
        {
            pointsDisplay.SetTextContent(randomMissedMessage);
            pointsDisplay.SetTextColor(missedMessageColor);
            isShowingMissedMessage = true;
        }
    }

    public void ShowGrabTromboneMessage()
    {
        if (messageDisplay != null)
        {
            messageDisplay.FreeText();
            messageDisplay.SetTextContent(grabTromboneMessage);
        }
    }

    public void ShowCountdown(int time)
    {
        if (messageDisplay != null)
        {
            messageDisplay.FreeText();
            if (time > 0)
            {
                if (time <= startCountDownAt)
                    messageDisplay.SetTextContent(time.ToString());
            }
        }
    }
}
