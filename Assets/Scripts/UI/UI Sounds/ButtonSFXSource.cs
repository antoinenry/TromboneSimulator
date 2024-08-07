using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSFXSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ButtonSFX sfx;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public virtual void OnPointerEnter(PointerEventData eventData) => MenuUI.SFXSource?.Play(sfx?.pointerEnterSFX);
    public virtual void OnPointerExit(PointerEventData eventData) => MenuUI.SFXSource?.Play(sfx?.pointerExitSFX);
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (button == null || button.interactable) MenuUI.SFXSource?.Play(sfx?.pointerClickSFX);
        else MenuUI.SFXSource?.Play(sfx?.notInteractableSFX);
    }
}

[CreateAssetMenu(menuName = "Trombone Hero/UI SFX/Button")]
public class ButtonSFX : PointerTargetSFX
{
    public AudioClip notInteractableSFX;
}