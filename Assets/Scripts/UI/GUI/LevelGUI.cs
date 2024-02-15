using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteAlways]
public class LevelGUI : GameUI
{
    [Header("UI Components")]
    public Button pauseButton;    
    public Slider timeBar;
    public SwooshDisplay messageDisplay;
    [Header("Content")]
    public string grabTromboneMessage = "a vos coulisses";
    public int startCountDownAt = 3;
    [Header("Events")]
    public UnityEvent onPauseRequest;

    private LevelLoader levelLoader;

    public override Component[] UIComponents => new Component[] 
        { pauseButton, timeBar, messageDisplay };

    private void OnClickPauseButton()
    {
        onPauseRequest.Invoke();
    }

    public void SetTimeBar(float timeValue, float timeMax)
    {
        if (timeBar)
        {
            timeBar.value = timeValue;
            timeBar.maxValue = timeMax;
        }
    }

    public void SetPauseButtonActive(bool enable)
    {
        if (pauseButton)
        {
            if (enable != pauseButton.interactable)
            {
                if (enable) pauseButton.onClick.AddListener(OnClickPauseButton);
                else pauseButton.onClick.RemoveListener(OnClickPauseButton);
            }
            pauseButton.interactable = enable;
        }
    }

    public void ShowGrabTromboneMessage()
    {
        if (messageDisplay)
        {
            messageDisplay.FreeText();
            messageDisplay.SetTextContent(grabTromboneMessage);
        }
    }

    public void ShowCountdown(int time)
    {
        if (messageDisplay)
        {
            messageDisplay.FreeText();
            if (time > 0)
            {
                if (time <= startCountDownAt)
                    messageDisplay.SetTextContent(time.ToString());
            }
        }
    }

    public void ClearMessage()
    {
        if (messageDisplay) messageDisplay.FreeText();
    }
}
