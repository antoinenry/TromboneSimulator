using UnityEngine;
using UnityEngine.EventSystems;

public class PointerTargetSFXSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PointerTargetSFX sfx;

    public void OnPointerEnter(PointerEventData eventData) => MenuUI.SFXSource?.Play(sfx?.pointerEnterSFX);
    public void OnPointerExit(PointerEventData eventData) => MenuUI.SFXSource?.Play(sfx?.pointerExitSFX);
    public void OnPointerClick(PointerEventData eventData) => MenuUI.SFXSource?.Play(sfx?.pointerClickSFX);
}

[CreateAssetMenu(menuName = "Trombone Hero/UI SFX/Pointer Target")]
public class PointerTargetSFX : ScriptableObject
{
    public AudioClip pointerEnterSFX;
    public AudioClip pointerExitSFX;
    public AudioClip pointerClickSFX;
}
