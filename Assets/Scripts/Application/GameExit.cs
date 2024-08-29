using UnityEngine;
using static DialogBoxScreen;

public class GameExit : MonoBehaviour
{
    public bool showDialog;
    public Dialog dialog;

    private DialogBoxScreen dialogBox;

    public void Exit()
    {
        if (showDialog)
        {
            dialogBox = MenuUI.Get<DialogBoxScreen>();
            if (dialogBox != null)
            {
                dialogBox.configuration = dialog;
                dialogBox.onAnswer.AddListener(OnDialogAnswer);
                dialogBox.ShowUI();
                return;
            }
        }
        QuitApplication();
    }

    private void OnDialogAnswer(bool answer)
    {
        if (dialogBox) dialogBox.onAnswer.RemoveListener(OnDialogAnswer);
        if (answer == true) QuitApplication();
    }

    private void QuitApplication()
    {
        if (Application.isEditor)
        {
            Debug.Log("Application Quit");
            Debug.Break();
        }
        else Application.Quit();
    }
}