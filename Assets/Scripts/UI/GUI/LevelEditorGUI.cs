using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

[ExecuteAlways]
public class LevelEditorGUI : GameUI
{
    [Header("UI Components")]
    public Slider timeBar;
    [Header("Events")]
    public UnityEvent<float> onMoveTimeBar;

    private void OnEnable()
    {
        if (timeBar != null) timeBar.onValueChanged.AddListener(OnMoveTimeBar);
    }

    private void OnDisable()
    {
        if (timeBar != null) timeBar.onValueChanged.RemoveListener(OnMoveTimeBar);
    }

    public override bool GUIActive
    {
        get
        {
            if (timeBar == null || timeBar.gameObject.activeInHierarchy == false) return false;
            return true;
        }

        set
        {
            if (timeBar != null)
            {
                timeBar.gameObject.SetActive(value);
                if (value) timeBar.interactable = true;
            }
        }
    }

    private void OnMoveTimeBar(float value)
    {
        onMoveTimeBar.Invoke(value / timeBar.maxValue);
    }

    public void SetTimeBar(float time, float maxTime)
    {
        if (timeBar != null) timeBar.SetValueWithoutNotify((time / maxTime) * timeBar.maxValue);
    }
}
