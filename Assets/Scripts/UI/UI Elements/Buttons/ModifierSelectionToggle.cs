using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class ModifierSelectionToggle : SelectionToggle<TromboneBuildModifier>
{
    [Header("Components")]
    public Image icon;
    public Image lockOverlay;
    [Header("Content")]
    public TromboneBuildModifier modifierAsset;
    public bool isLocked;

    public override TromboneBuildModifier Selection
    { 
        get => modifierAsset; 
        set => SetModifier(value);
    }

    private void Update()
    {
        if (toggleButton != null) toggleButton.enabled = !isLocked;
        SetToggleLook();
    }

    public void AddListeners(UnityAction<TromboneBuildModifier, bool> onSelectAction, UnityAction<TromboneBuildModifier, bool> onClickAction)
    {
        if (onSelectAction != null) onHover.AddListener(onSelectAction);
        if (onClickAction != null) onToggle.AddListener(onClickAction);
    }

    public void RemoveListeners(UnityAction<TromboneBuildModifier, bool> onSelectAction, UnityAction<TromboneBuildModifier, bool> onClickAction)
    {
        if (onSelectAction != null) onHover.RemoveListener(onSelectAction);
        if (onClickAction != null) onToggle.RemoveListener(onClickAction);
    }

    public void SetModifier(TromboneBuildModifier modifier)
    {
        modifierAsset = modifier;
        if (icon)
        {
            icon.sprite = modifier.icon;
            icon.color = modifier.IconColor;
        }
    }

    public override void Click()
    {
        if (isLocked) return;
        base.Click();
    }

    public override void SetToggleLook()
    {
        base.SetToggleLook();
        if (lockOverlay) lockOverlay.enabled = isLocked;
    }
}
