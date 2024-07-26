using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PauseScreen : MenuUI
{
    [Header("Components")]
    public Button stopButton;
    public Button playButton;
    public Button settingsButton;
    [Header("Configuration")]
    public DialogBoxScreen.Dialog quitDialog;
    [Header("Events")]
    public UnityEvent onUnpause;
    public UnityEvent onQuit;
    public UnityEvent onOpenSettings;

    private DialogBoxScreen dialogBox;

    protected override void Awake()
    {
        base.Awake();
        dialogBox = Get<DialogBoxScreen>();
    }

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
        stopButton.onClick.AddListener(RequestQuit);
        playButton.onClick.AddListener(Unpause);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    private void DisableButtons()
    {
        stopButton.onClick.RemoveListener(RequestQuit);
        playButton.onClick.RemoveListener(Unpause);
        settingsButton.onClick.RemoveListener(OpenSettings);
    }

    private void Unpause()
    {
        onUnpause.Invoke();
        HideUI();
    }

    private void RequestQuit()
    {
        if (dialogBox != null)
        {
            dialogBox.configuration = quitDialog;
            dialogBox.onAnswer.AddListener(OnQuitAnswer);
            dialogBox.ShowUI();
        }
        else
            ValidateQuit();
    }

    private void OnQuitAnswer(bool quit)
    {
        if (dialogBox != null) dialogBox.onAnswer.RemoveListener(OnQuitAnswer);
        if (quit) ValidateQuit();
    }

    private void ValidateQuit()
    {
        onQuit.Invoke();
        HideUI();
    }

    private void OpenSettings()
    {

    }
}
