using UnityEngine;

public class ModifierUnlockScreen : MenuUI
{
    [Header("Components")]
    public ModifierUnlockButton modifierButtonPrefab;
    public RectTransform modifierButtonContainer;
    [Header("Configuration")]
    public TromboneBuildModifier[] modifiers;
    public float modifierContainerSizeMargin = 0f;

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

    private void OnEnable()
    {
        UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        AddButtonListeners(buttons);
    }

    private void OnDisable()
    {
        UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        RemoveButtonListeners(buttons);
    }

    protected override void Update()
    {
        base.Update();
        bool buttonChanges = UpdateButtonInstances(out ModifierUnlockButton[] buttons);
        UpdateButtonContent(buttons);
        UpdateButtonContainerSize(buttons.Length);
        if (buttonChanges && Application.isPlaying) AddButtonListeners(buttons);
    }

    public override void ShowUI()
    {
        base.ShowUI();
        GameContentPicker.PickModifiers(ref modifiers, progress:GameProgress.Current);
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

    private void AddButtonListeners(ModifierUnlockButton[] buttons)
    {
        if (buttons == null || buttons.Length == 0) return;
        foreach (ModifierUnlockButton button in buttons) button?.onClick?.AddListener(OnClickModifierButton);
    }

    private void RemoveButtonListeners(ModifierUnlockButton[] buttons)
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
        GameProgress.Current?.TrySetLock(modifier, false);
    }
}
