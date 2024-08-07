using TMPro;
using UnityEngine;

public class ModifierUnlockScreen : MenuUI
{
    [Header("Components")]
    public TromboneBuildStack modifierStack;
    public ModifierUnlockButton modifierButtonPrefab;
    public RectTransform modifierButtonContainer;
    public TMP_Text infoTextField;
    public GameContentPicker gameContentPicker;
    [Header("Configuration")]
    public TromboneBuildModifier[] modifiers;
    public int requestModifierCount = 3;
    public float modifierContainerSizeMargin = 0f;
    public DialogBoxScreen.Dialog applyModDialog;
    public string infoDefaultText = "";
    public bool infoShowsDescription = true;
    public bool infoShowsStats = false;

    private TromboneBuildModifier selectedModifier;
    private TromboneBuildModifier clickedModifier;
    private DialogBoxScreen dialogBox;

    private float ModifierButtonWidth
    {
        get
        {
            RectTransform rt = modifierButtonPrefab?.GetComponent<RectTransform>();
            return rt != null ? rt.rect.width : 0f;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        dialogBox = Get<DialogBoxScreen>();
    }

    protected override void Update()
    {
        base.Update();
        bool buttonChanges = UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        UpdateButtonContent(buttons);
        UpdateButtonContainerSize(buttons.Length);
        if (buttonChanges && Application.isPlaying) AddModifierButtonsListeners(buttons);
    }

    public override void ShowUI()
    {
        base.ShowUI();
        PickModifiers();
        if (Application.isPlaying && (modifiers == null || modifiers.Length == 0))
        {
            HideUI();
            return;
        }
        UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        AddModifierButtonsListeners(buttons);
        //SelectModifier(buttons, 0);
    }

    public override void HideUI()
    {
        base.HideUI();
        UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        RemoveModifiersButtonsListeners(buttons);
    }

    private bool UpdateButtonInstances(out ModifierUnlockButton[] buttons)
    {
        // Check correct number of instances
        buttons = modifierButtonContainer != null ? modifierButtonContainer.GetComponentsInChildren<ModifierUnlockButton>(true) : new ModifierUnlockButton[0];
        int modifierCount = modifiers != null ? modifiers.Length : 0;
        if (buttons.Length == modifierCount) return false;
        // Destroy current buttons
        foreach (ModifierUnlockButton button in buttons) if (button != null) DestroyImmediate(button.gameObject);
        // Create new buttons
        buttons = new ModifierUnlockButton[modifierCount];
        for (int i = 0; i < modifierCount; i++) buttons[i] = Instantiate(modifierButtonPrefab, modifierButtonContainer);
        return true;
    }

    private void UpdateButtonContent(ModifierUnlockButton[] buttons)
    {
        int modifierCount = modifiers != null ? modifiers.Length : 0;
        int buttonCount = buttons != null ? buttons.Length : 0;
        for (int i = 0; i < modifierCount; i++)
        {
            if (i >= buttonCount)
            {
                Debug.LogWarning("Buttons/modifiers count mismatch");
                break;
            }
            if (buttons[i] == null) continue;
            buttons[i].modifier = modifiers[i];
        }
    }

    private void SelectModifier(ModifierUnlockButton[] buttons, int index)
    {
        int buttonCount = buttons != null ? buttons.Length : 0;
        if (index < 0 || index >= buttonCount || buttons[index] == null) return;
        selectedModifier = buttons[index].modifier;
        buttons[index].Select();
    }

    private void AddModifierButtonsListeners(ModifierUnlockButton[] buttons)
    {
        if (buttons == null || buttons.Length == 0) return;
        foreach (ModifierUnlockButton button in buttons) button?.AddListeners(OnSelectModifierButton, OnClickModifierButton);
    }

    private void RemoveModifiersButtonsListeners(ModifierUnlockButton[] buttons)
    {
        if (buttons == null || buttons.Length == 0) return;
        foreach (ModifierUnlockButton button in buttons) button?.RemoveListeners(OnSelectModifierButton, OnClickModifierButton);
    }

    private void UpdateButtonContainerSize(int buttonCount)
    {
        if (modifierButtonContainer == null) return;
        Vector2 size = modifierButtonContainer.sizeDelta;
        size.x = buttonCount * ModifierButtonWidth + modifierContainerSizeMargin;
        modifierButtonContainer.sizeDelta = size;
    }

    private void OnSelectModifierButton(TromboneBuildModifier modifier, bool selected)
    {
        if (infoTextField)
        {
            if (selected) ShowModifierInfo(modifier);
            else if (modifier == selectedModifier) infoTextField.SetText(infoDefaultText);
        }
        if (selected) selectedModifier = modifier;
        else if (selectedModifier == modifier) selectedModifier = null;
    }

    private void OnClickModifierButton(TromboneBuildModifier modifier)
    {
        clickedModifier = modifier;
        if (clickedModifier == null)
        {
            HideUI();
            return;
        }
        GameProgress.Current?.TrySetLock(clickedModifier, false);
        if (dialogBox != null)
        {
            dialogBox.configuration = applyModDialog;
            dialogBox.configuration.bottomText = clickedModifier.modName;
            dialogBox.onAnswer.AddListener(OnApplyAnswer);
            dialogBox.ShowUI();
        }
        else
            HideUI();
    }

    private void OnApplyAnswer(bool apply)
    {
        if (dialogBox != null) dialogBox.onAnswer.RemoveListener(OnApplyAnswer);
        if (apply)
        {
            if (modifierStack == null) return;
            modifierStack.TryAddModifier(clickedModifier);
            modifierStack.ApplyStack();
        }
        HideUI();
    }

    private void ShowModifierInfo(TromboneBuildModifier modifier)
    {
        if (infoTextField == null) return;
        string infoText = infoDefaultText;
        if (modifier != null)
        {
            if (infoShowsDescription && infoShowsStats) infoText = modifier.description + "\n" + modifier.StatDescription;
            else if (infoShowsDescription) infoText = modifier.description;
            else if (infoShowsStats) infoText = modifier.StatDescription;
        }
        infoTextField.SetText(infoText);
    }

    private void PickModifiers()
    {
        if (requestModifierCount < 0) requestModifierCount = 0;
        if (gameContentPicker != null)
        {
            gameContentPicker.picks = new TromboneBuildModifier[requestModifierCount];
            gameContentPicker.Pick();
            modifiers = gameContentPicker.picks;
        }
        else
        {
            modifiers = GameContentPicker.PickUnlockableModifiers(requestModifierCount, GameProgress.Current, prioritizeHighTier: true);
        }
    }
}
