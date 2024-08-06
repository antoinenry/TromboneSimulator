using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class ModifierSelectionButton : MonoBehaviour
{
    public enum ModifierAvailability { Locked, Available, Active }

    [Header("Components")]
    public Button button;
    public Image icon;
    public Image lockOverlay;
    [Header("Content")]
    public TromboneBuildModifier modifierAsset;
    public ModifierAvailability availability;
    [Header("Active colors")]
    public ColorBlock activeColors = ColorBlock.defaultColorBlock;
    [Header("Inactive colors")]
    public ColorBlock inactiveColors = ColorBlock.defaultColorBlock;

    public UnityEvent<TromboneBuildModifier,bool> onSelect;
    public UnityEvent<TromboneBuildModifier, bool> onToggle;

    private UIToggleSFX sfxSource;

    private void Awake()
    {
        sfxSource = GetComponentInChildren<UIToggleSFX>(true);
    }

    private void Update()
    {
        SetButtonLook();
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
        if (availability == ModifierAvailability.Locked) return;
        bool activate = availability == ModifierAvailability.Available;
        availability = activate ? ModifierAvailability.Active : ModifierAvailability.Available;
        sfxSource.OnToggle(activate);
        onToggle.Invoke(modifierAsset, activate);
    }

    public void OnSelect()
    {
        onSelect.Invoke(modifierAsset, true);
    }

    public void OnUnselect()
    {
        onSelect.Invoke(modifierAsset, false);
    }

    public void SetButtonLook()
    {
        if (lockOverlay) lockOverlay.enabled = availability == ModifierAvailability.Locked;
        if (button) button.colors = availability == ModifierAvailability.Active ? activeColors : inactiveColors;
    }
}
