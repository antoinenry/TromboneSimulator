using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class SelectionButton<T> : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent<T, bool> onHover;
    public UnityEvent<T> onClick;

    protected Button button;

    public abstract T Selection { get; set; }

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData) => Click();
    public void OnPointerEnter(PointerEventData eventData) => Select();
    public void OnPointerExit(PointerEventData eventData) => Unselect();

    public virtual void AddListeners(UnityAction<T, bool> onSelectAction, UnityAction<T> onClickAction)
    {
        if (onSelectAction != null) onHover.AddListener(onSelectAction);
        if (onClickAction != null) onClick.AddListener(onClickAction);
    }

    public virtual void RemoveListeners(UnityAction<T, bool> onSelectAction, UnityAction<T> onClickAction)
    {
        if (onSelectAction != null) onHover.RemoveListener(onSelectAction);
        if (onClickAction != null) onClick.RemoveListener(onClickAction);
    }

    public virtual void Click()
    {
        if (button == null || button.interactable == false) return;
        onClick.Invoke(Selection);
    }

    public virtual void Select()
    {
        if (button == null || button.interactable == false) return;
        button?.Select();
        onHover.Invoke(Selection, true);
    }

    public virtual void Unselect()
    {
        if (button == null || button.interactable == false) return;
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject == gameObject) eventSystem.SetSelectedGameObject(null);
        onHover.Invoke(Selection, false);
    }
}