using UnityEngine;
using UnityEngine.UI;

public class ModifierUnlockScreen : MenuUI
{
    [Header("Components")]
    public TromboneBuildStack modifierStack;
    public ModifierUnlockButton modifierButtonPrefab;
    public RectTransform modifierButtonContainer;
    public Button validateButton;
    [Header("Configuration")]
    public TromboneBuildModifier[] modifiers;
    public float modifierContainerSizeMargin = 0f;
    public DialogBoxScreen.Dialog applyModDialog;

    private TromboneBuildModifier selectedModifier;
    private DialogBoxScreen dialogBox;

    private float ModifierButtonWidth
    {
        get
        {
            RectTransform rt = modifierButtonPrefab?.GetComponent<RectTransform>();
            return rt != null ? rt.rect.width : 0f;
        }
    }

    private void OnValidate()
    {
        Update();
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
        validateButton?.onClick?.AddListener(OnClickValidateButton);
        GameContentPicker.PickModifiers(ref modifiers, progress:GameProgress.Current);
        UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        AddModifierButtonsListeners(buttons);
        SelectModifier(buttons, 0);
    }

    public override void HideUI()
    {
        base.HideUI();
        validateButton?.onClick?.RemoveListener(OnClickValidateButton);
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
        foreach (ModifierUnlockButton button in buttons) button?.onClick?.AddListener(OnClickModifierButton);
    }

    private void RemoveModifiersButtonsListeners(ModifierUnlockButton[] buttons)
    {
        if (buttons == null || buttons.Length == 0) return;
        foreach (ModifierUnlockButton button in buttons) button?.onClick?.RemoveListener(OnClickModifierButton);
    }

    private void UpdateButtonContainerSize(int buttonCount)
    {
        if (modifierButtonContainer == null) return;
        Vector2 size = modifierButtonContainer.sizeDelta;
        size.x = buttonCount * ModifierButtonWidth + modifierContainerSizeMargin;
        modifierButtonContainer.sizeDelta = size;
    }

    private void OnClickModifierButton(TromboneBuildModifier modifier)
    {
        selectedModifier = modifier;
    }

    private void OnClickValidateButton()
    {
        GameProgress.Current?.TrySetLock(selectedModifier, false);
        if (dialogBox != null && selectedModifier != null)
        {
            dialogBox.configuration = applyModDialog;
            dialogBox.configuration.bottomText = selectedModifier.modName;
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
            modifierStack.TryAddModifier(selectedModifier);
            modifierStack.ApplyStack();
        }
        HideUI();
    }
}
