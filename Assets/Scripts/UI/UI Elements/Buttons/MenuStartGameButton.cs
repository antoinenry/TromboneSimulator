using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuStartGameButton : MenuNavigationButton
{
    public bool showDialog;
    public DialogBoxScreen.Dialog dialog;

    private DialogBoxScreen dialogBox;

    protected override void Awake()
    {
        base.Awake();    
        dialogBox = MenuUI.Get<DialogBoxScreen>();
    }

    override protected void OnButtonClicked()
    {
        if (showDialog) RequestStart();
        else ValidateStart();        
    }

    private void ValidateStart()
    {
        origin?.HideUI();
        MenuUI.onStartLevel.Invoke();
    }

    private void RequestStart()
    {
        if (dialogBox != null)
        {
            dialogBox.configuration = dialog;
            Level l = MenuUI.Get<LevelSelectionScreen>()?.GetSelectedLevel();
            dialogBox.configuration.messageText += l != null ? l.name : "";
            dialogBox.onAnswer.AddListener(OnConfirmAnswer);
            dialogBox.ShowUI();
        }
        else
            ValidateStart();
    }

    private void OnConfirmAnswer(bool quit)
    {
        if (dialogBox != null) dialogBox.onAnswer.RemoveListener(OnConfirmAnswer);
        if (quit) ValidateStart();
    }
}