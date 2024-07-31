using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public int soundIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        MenuUI.SFXSource?.PlayButtonHoverSFX(soundIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MenuUI.SFXSource?.PlayButtonClickSFX(soundIndex);
    }
}
