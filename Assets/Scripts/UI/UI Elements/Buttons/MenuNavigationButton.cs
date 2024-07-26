using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class MenuNavigationButton : MonoBehaviour
{
    public MenuUI origin;

    private Button button;

    protected virtual void Reset()
    {
        TryFindOrigin();
    }

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
    }

    protected virtual void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    protected virtual void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    private void TryFindOrigin()
    {
        Transform parent = transform;
        while (origin == null && parent != null)
        {
            parent = parent.parent;
            origin = parent.GetComponent<MenuUI>();
        }
    }

    protected abstract void OnButtonClicked();
}