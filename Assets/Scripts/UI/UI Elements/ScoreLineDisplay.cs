using UnityEngine;
using TMPro;
using System;

[ExecuteAlways]
public class ScoreLineDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] textDisplays;
    public CounterDisplay[] valueDisplays;
    public bool visible;

    public bool IsMoving
    {
        get
        {
            if (valueDisplays != null)
            {
                foreach (CounterDisplay cd in valueDisplays)
                    if (cd.IsMoving) return true;
            }
            return false;
        }
    }

    private void Reset()
    {
        textDisplays = GetComponentsInChildren<TextMeshProUGUI>(true);
        textDisplays = Array.FindAll(textDisplays, t => t.GetComponent<CounterDisplay>() == null);
        valueDisplays = GetComponentsInChildren<CounterDisplay>(true);
    }

    public void Update()
    {
        foreach (TextMeshProUGUI t in textDisplays) t.enabled = visible;
        foreach (CounterDisplay v in valueDisplays) v.enabled = visible;
    }

    public void SetTextAt(int index, string text)
    {
        if (textDisplays == null || index < 0 || index >= textDisplays.Length) return;
        else textDisplays[index].text = text;
    }

    public void SetValueAt(int index, float value)
    {
        if (valueDisplays == null || index < 0 || index >= valueDisplays.Length) return;
        else valueDisplays[index].value = value;

        if (Application.isPlaying == false)
        {
            foreach (CounterDisplay d in valueDisplays) d.Update();
        }
    }

    public void SetTexts(params string[] texts)
    {
        for (int i = 0, iend = texts.Length; i < iend; i++)
            SetTextAt(i, texts[i]);
    }

    public void SetValues(params float[] values)
    {
        for (int i = 0, iend = values.Length; i < iend; i++)
            SetValueAt(i, values[i]);
    }

    public void ResetValues()
    {
        foreach (CounterDisplay d in valueDisplays) d.SetValueImmediate(0f);
    }
}
