using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[ExecuteAlways]
public class LevelGUI : GameUI
{
    [Header("UI Components")]
    public Button pauseButton;    
    public Slider timeBar;
    public SwooshDisplay messageDisplay;
    public RectTransform eventPanel;
    [Header("Content")]
    public string grabTromboneMessage = "a vos coulisses";
    public int startCountDownAt = 3;
    [Header("Events")]
    public UnityEvent onPauseRequest;

    private LevelLoader levelLoader;
    private DanceAnimation messageDance;

    public string CurrentMessage => messageDisplay != null ? messageDisplay.FreshText : null;

    public override Component[] UIComponents => new Component[] 
        { pauseButton, timeBar, messageDisplay };

    private void Awake()
    {
        messageDance = messageDisplay != null ? messageDisplay.GetComponent<DanceAnimation>() : null;
    }

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

    public void ShowMessage(string text, bool dance = false)
    {
        if (messageDisplay && messageDisplay.FreshText != text)
        {
            messageDisplay.FreeText();
            messageDisplay.SetTextContent(text);
            if (messageDance != null) messageDance.enabled = dance;
        }
    }

    public void FreeMessage(string text)
    {
        if (messageDisplay && messageDisplay.FreshText == text) messageDisplay.FreeText();
    }

    public void ShowMessage(string text, float duration, bool dance = false) => StartCoroutine(ShowTimedMessageCoroutine(text, duration, dance));

    private IEnumerator ShowTimedMessageCoroutine(string text, float duration, bool dance)
    {
        ShowMessage(text, dance);
        yield return new WaitForSeconds(duration);
        FreeMessage(text);
    }

    public void ShowGrabTromboneMessage() => ShowMessage(grabTromboneMessage);

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
