using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSFXSource : PointerTargetSFXSource
{
    protected Button button;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (IsButtonInteractable) base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (IsButtonInteractable) base.OnPointerExit(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (HasMatchingSFX)
        {
            ButtonSFX buttonSFX = sfx as ButtonSFX;
            MenuUI.SFXSource?.Play(IsButtonInteractable ? buttonSFX?.pointerClick : buttonSFX?.notInteractable);
        }
        else
            base.OnPointerClick(eventData);
    }

    protected virtual bool HasMatchingSFX => sfx != null && sfx is ButtonSFX;

    protected virtual bool IsButtonInteractable => button != null && button.interactable;
}