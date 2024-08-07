using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

[ExecuteAlways]
public class ToggleButton : MonoBehaviour
{
    public Button button;
    public bool toggleActive;
    [Header("Active colors")]
    public ColorBlock activeColors = ColorBlock.defaultColorBlock;
    [Header("Inactive colors")]
    public ColorBlock inactiveColors = ColorBlock.defaultColorBlock;

    public UnityEvent<bool> onToggle;

    private void OnEnable()
    {
        button.interactable = true;
        button?.onClick?.AddListener(Click);
    }

    private void OnDisable()
    {
        button.interactable = false;
        button?.onClick?.RemoveListener(Click);
    }

    private void Update()
    {
        SetToggleLook();
    }

    private void Click()
    {
        if (button == null || button.interactable == false) return;
        toggleActive = !toggleActive;
        onToggle.Invoke(toggleActive);
    }

    public void SetToggleLook()
    {
        if (button) button.colors = toggleActive ? activeColors : inactiveColors;
    }
}