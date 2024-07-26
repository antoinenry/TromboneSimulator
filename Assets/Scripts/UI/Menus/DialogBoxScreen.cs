using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogBoxScreen : MenuUI
{
    [Serializable]
    public struct Dialog
    {
        public bool showBackground;
        public bool showYesButton;
        public bool showNoButton;
        public string messageText;
        public string bottomText;
    }

    [Header("Components")]
    public RawImage background;
    public TMP_Text messageField;
    public TMP_Text bottomField;
    public Button yesButton;
    public Button noButton;
    [Header("Dialog")]
    public Dialog configuration;
    public UnityEvent<bool> onAnswer;

    protected override void Update()
    {
        base.Update();
        if (background) background.enabled = configuration.showBackground;
        ShowButton(yesButton, configuration.showYesButton);
        ShowButton(noButton, configuration.showNoButton);
        ShowText(messageField, configuration.messageText);
        ShowText(bottomField, configuration.bottomText);
    }

    public override void ShowUI()
    {
        base.ShowUI();
        if (Application.isPlaying) EnableButtons();
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying) DisableButtons();
    }

    private void ShowButton(Button b, bool show)
    {
        if (b == null) return;
        b.gameObject.SetActive(show);
    }

    private void ShowText(TMP_Text field, string  text)
    {
        if (field == null) return;
        if (text == null || text.Length == 0) field.gameObject.SetActive(false);
        else
        {
            field.gameObject.SetActive(true);
            field.text = text;
        }
    }

    private void EnableButtons()
    {
        yesButton?.onClick?.AddListener(OnClickYes);
        noButton?.onClick?.AddListener(OnClickNo);
    }

    private void DisableButtons()
    {
        yesButton?.onClick?.RemoveListener(OnClickYes);
        noButton?.onClick?.RemoveListener(OnClickNo);
    }

    private void OnClickYes()
    {
        onAnswer.Invoke(true);
        HideUI();
    }

    private void OnClickNo()
    {
        onAnswer.Invoke(false);
        HideUI();
    }
}
