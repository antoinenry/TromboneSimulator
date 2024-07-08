using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class ModifierSelectionButton : MonoBehaviour
{
    public Button button;
    public Image icon;
    [Header("Content")]
    public TromboneBuildModifier modifierAsset;
    public bool active;
    [Header("Active colors")]
    public ColorBlock activeColors = ColorBlock.defaultColorBlock;
    [Header("Inactive colors")]
    public ColorBlock inactiveColors = ColorBlock.defaultColorBlock;

    public UnityEvent<TromboneBuildModifier,bool> onSelect;
    public UnityEvent<TromboneBuildModifier, bool> onToggle;

    private void Update()
    {
        if (button) button.colors = active ? activeColors : inactiveColors;
    }

    public void AddListeners(UnityAction<TromboneBuildModifier, bool> onSelectAction, UnityAction<TromboneBuildModifier, bool> onClickAction)
    {
        if (onSelectAction != null) onSelect.AddListener(onSelectAction);
        if (onClickAction != null) onToggle.AddListener(onClickAction);
    }

    public void RemoveListeners(UnityAction<TromboneBuildModifier, bool> onSelectAction, UnityAction<TromboneBuildModifier, bool> onClickAction)
    {
        if (onSelectAction != null) onSelect.RemoveListener(onSelectAction);
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

    public void OnClick()
    {
        if (modifierAsset == null) return;
        active = !active;
        onToggle.Invoke(modifierAsset, active);
    }

    public void OnSelect()
    {
        onSelect.Invoke(modifierAsset, true);
    }

    public void OnUnselect()
    {
        onSelect.Invoke(modifierAsset, false);
    }
}
