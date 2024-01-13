using UnityEngine;
using TMPro;

[ExecuteAlways]
public class CounterDisplay : MonoBehaviour
{
    public TextMeshProUGUI textRenderer;
    public string format = "0";
    [Min(0)] public float incrementSpeed = 10f;
    [Min(0)] public float maxIncrementTime = 10f;
    public bool threeDigitsSpacing;
    public char pointFormat = '.';
    [Min(0)] public float value;

    public float CurrentDisplayValue { get; private set; }
    public bool IsMoving { get; private set; }

    public void Update()
    {
        if (textRenderer != null)
        {
            // Increment animation
            if (Application.isPlaying && incrementSpeed > 0f)
            {
                IsMoving = (CurrentDisplayValue != value);
                float estimatedTime = Mathf.Abs(CurrentDisplayValue - value) / incrementSpeed;
                if (estimatedTime <= maxIncrementTime)
                    CurrentDisplayValue = Mathf.MoveTowards(CurrentDisplayValue, value, incrementSpeed * Time.deltaTime);
                else
                    CurrentDisplayValue = Mathf.MoveTowards(CurrentDisplayValue, value, estimatedTime / Time.deltaTime);
            }
            else
            {
                CurrentDisplayValue = value;
                IsMoving = false;
            }
            // Convert value to string
            string valueString = CurrentDisplayValue.ToString(format);
            // Add spacing every 3 digits
            if (threeDigitsSpacing)
            {
                string spacedString = "";
                for (int i = 0, stringLength = valueString.Length; i < stringLength; i++)
                {
                    int j = stringLength - i - 1;
                    if (i % 3 == 0) spacedString = " " + spacedString;
                    spacedString = valueString[j] + spacedString;
                }
                valueString = spacedString;
            }
            valueString = valueString.Replace(',', pointFormat);
            // Display value
            textRenderer.text = valueString;
        }
    }

    private void OnEnable()
    {
        textRenderer.enabled = true;
    }

    private void OnDisable()
    {
        textRenderer.enabled = false;
    }

    public void SetValueImmediate(float setValue)
    {
        value = setValue;
        CurrentDisplayValue = value;
        Update();
    }
}
