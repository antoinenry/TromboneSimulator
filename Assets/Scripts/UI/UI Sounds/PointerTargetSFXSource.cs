using UnityEngine;
using UnityEngine.EventSystems;

public class PointerTargetSFXSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PointerTargetSFX sfx;

    public virtual void OnPointerEnter(PointerEventData eventData) => MenuUI.SFXSource?.PlayOneShot(sfx?.pointerEnter);
    public virtual void OnPointerExit(PointerEventData eventData) => MenuUI.SFXSource?.PlayOneShot(sfx?.pointerExit);
    public virtual void OnPointerClick(PointerEventData eventData) => MenuUI.SFXSource?.PlayOneShot(sfx?.pointerClick);
}
