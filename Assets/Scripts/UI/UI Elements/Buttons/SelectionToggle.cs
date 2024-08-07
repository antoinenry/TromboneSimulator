using UnityEngine.Events;
using UnityEngine;

[RequireComponent(typeof(ToggleButton))]
public abstract class SelectionToggle<T> : SelectionButton<T>
{
    public UnityEvent<T,bool> onToggle;

    protected ToggleButton toggleButton;

    protected override void Awake()
    {
        toggleButton = GetComponent<ToggleButton>();
        button = toggleButton?.button;
    }

    protected virtual void OnEnable()
    {
        toggleButton?.onToggle?.AddListener(OnToggle);
        SetToggleLook();
    }

    protected virtual void OnDisable()
    {
        toggleButton?.onToggle?.RemoveListener(OnToggle);
    }

    protected virtual void OnToggle(bool value)
    {
        onToggle.Invoke(Selection, value);
        SetToggleLook();
    }

    public virtual void SetToggle(bool value)
    {
        if (toggleButton == null || toggleButton.toggleActive == value) return;
        toggleButton.toggleActive = value;
        OnToggle(value);
        SetToggleLook();
    }

    public virtual void SetToggleLook()
    {
        toggleButton?.SetToggleLook();
    }
}