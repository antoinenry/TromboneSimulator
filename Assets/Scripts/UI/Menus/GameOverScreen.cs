using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using static DialogBoxScreen;

public class GameOverScreen : MenuUI
{
    [Header("UI Components")]
    public TMP_Text textField;
    public Button replayButton;
    public Button quitButton;
    public Animation animate;
    [Header("Look")]
    public string defaultText = "Game Over";
    public Color defaultTextColor = Color.red;
    public string replayText = "Try Again?";
    public Color replayTextColor = Color.yellow;
    public string quitText = "Give Up?";
    public Color quitTextColor = Color.white;
    public Dialog quitDialog;
    [Header("Animation")]
    public AnimationClip showAnimation;
    public float showAnimationDuration = 4f;
    public AnimationClip selectReplayAnimation;
    public AnimationClip selectQuitAnimation;
    public AnimationClip replayAnimation;
    public AnimationClip quitAnimation;
    public float hideAnimationDuration = 2f;
    [Header("Events")]
    public UnityEvent<bool> onContinue;

    private DialogBoxScreen dialogBox;

    protected override void Awake()
    {
        base.Awake();
        dialogBox = Get<DialogBoxScreen>();
    }

    public override void ShowUI()
    {
        if (Application.isPlaying)
            StartCoroutine(ShowUICoroutine());
        else
        {
            base.ShowUI();
            SetButtonsActive(true);
            OnSelectNothing();
        }
    }

    private void PlayAnimation(AnimationClip clip, bool loop = false)
    {
        if (animate == null || clip == null) return;
        animate.clip = clip;
        animate.Play();
    }

    private IEnumerator ShowUICoroutine()
    {
        base.ShowUI();
        SetButtonsActive(false);
        OnSelectNothing();
        PlayAnimation(showAnimation);
        yield return new WaitForSeconds(showAnimationDuration);
        SetButtonsActive(true);
    }

    private IEnumerator HideUICoroutine(bool replay)
    {
        SetButtonsActive(false);
        PlayAnimation(replay ? replayAnimation : quitAnimation);
        yield return new WaitForSeconds(hideAnimationDuration);
        onContinue.Invoke(replay);
        HideUI();
    }

    private void SetButtonsActive(bool visible)
    {
        replayButton?.gameObject?.SetActive(visible);
        quitButton?.gameObject?.SetActive(visible);
    }

    public void OnSelectNothing()
    {
        if (textField)
        {
            textField.SetText(defaultText);
            textField.color = defaultTextColor;
        }
    }

    public void OnSelectReplay()
    {
        if (textField)
        {
            textField.SetText(replayText);
            textField.color = replayTextColor;
        }
        PlayAnimation(selectReplayAnimation);
    }

    public void OnSelectQuit()
    {
        if (textField)
        {
            textField.SetText(quitText);
            textField.color = quitTextColor;
        }
        PlayAnimation(selectQuitAnimation);
    }

    public void OnPressReplay()
    {
        StartCoroutine(HideUICoroutine(replay: true));
    }

    public void OnPressQuit()
    {
        if (dialogBox != null)
        {
            dialogBox.configuration = quitDialog;
            dialogBox.onAnswer.AddListener(OnQuitDialogAnswer);
            dialogBox.ShowUI();
        }
        else
        {
            OnConfirmQuit();
        }
    }

    private void OnQuitDialogAnswer(bool answer)
    {
        dialogBox.onAnswer.RemoveListener(OnQuitDialogAnswer);
        if (answer == true) OnConfirmQuit();
    }

    private void OnConfirmQuit() => StartCoroutine(HideUICoroutine(replay: false));
}
