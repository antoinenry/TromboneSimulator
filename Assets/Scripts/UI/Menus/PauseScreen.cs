using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PauseScreen : MenuUI
{
    public Button stopButton;
    public Button playButton;
    public Button settingsButton;

    public UnityEvent onUnpause;
    public UnityEvent onQuit;
    public UnityEvent onOpenSettings;

    override public void ShowUI()
    {
        base.ShowUI();
        if (Application.isPlaying) EnableButtons();
        // Detach hand cursor from trombone
        if (cursor != null) cursor.cursorState &= ~HandCursor.CursorState.Trombone;
    }

    override public void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying) DisableButtons();
    }

    private void EnableButtons()
    {
        stopButton.onClick.AddListener(Quit);
        playButton.onClick.AddListener(Unpause);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    private void DisableButtons()
    {
        stopButton.onClick.RemoveListener(Quit);
        playButton.onClick.RemoveListener(Unpause);
        settingsButton.onClick.RemoveListener(OpenSettings);
    }

    private void Unpause()
    {
        onUnpause.Invoke();
        HideUI();
    }

    private void Quit()
    {
        onQuit.Invoke();
        HideUI();
    }

    private void OpenSettings()
    {

    }
}
