using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class GameOverScreen : MenuUI
{
    [Header("UI Components")]
    public TextMeshProUGUI continueDisplay;
    public Button continueButton;
    public TextMeshProUGUI continueButtonText;
    public Button giveUpButton;
    public string continueText = "Interpreter un morceau supplementaire !";
    public string gameOverText = "Game Over";
    [Header("Events")]
    public UnityEvent<bool> onContinue;

    //override protected void Start()
    //{
    //    if (Application.isPlaying)
    //    {
    //        levelLoader = FindObjectOfType<LevelLoader>(true);
    //        if (levelLoader != null) levelLoader.onGameOver.AddListener(ShowUI);
    //    }
    //    base.Start();
    //}

    public override void ShowUI()
    {
        if (Application.isPlaying)
        {
            if (continueButton != null) continueButton.onClick.AddListener(Continue);
            if (giveUpButton != null) giveUpButton.onClick.AddListener(GiveUp);
        }
        // Detach hand cursor from trombone
        if (cursor != null) cursor.cursorState &= ~HandCursor.CursorState.Trombone;
        base.ShowUI();
    }

    public override void HideUI()
    {
        if (Application.isPlaying)
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(Continue);
            if (giveUpButton != null) giveUpButton.onClick.RemoveListener(GiveUp);
        }
        base.HideUI();
    }

    public void DisplayGameOver(int continues)
    {
        continueDisplay.text = "x" + continues.ToString();
        if (continues > 0)
        {
            continueButtonText.text = continueText;
            continueButton.interactable = true;
        }
        else
        {
            continueButtonText.text = gameOverText;
            continueButton.interactable = false;
        }
        ShowUI();
    }

    public void DisplayGameOver()
    {
        continueDisplay.text = "";
        continueButtonText.text = continueText;
        continueButton.interactable = true;
        ShowUI();
    }

    private void Continue()
    {
        onContinue.Invoke(true);
        HideUI();
    }

    private void GiveUp()
    {
        onContinue.Invoke(false);
        HideUI();
    }
}
