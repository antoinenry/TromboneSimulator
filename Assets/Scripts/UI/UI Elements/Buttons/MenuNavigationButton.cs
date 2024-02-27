using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class MenuNavigationButton : MonoBehaviour
{
    public MenuUI origin;

    private Button button;

    private void Reset()
    {
        TryFindOrigin();
    }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
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