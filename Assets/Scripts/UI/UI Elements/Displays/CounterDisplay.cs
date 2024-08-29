using UnityEngine;
using TMPro;

[ExecuteAlways]
public class CounterDisplay : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI textRenderer;
    [Header("Format")]
    public string format = "0";
    public bool threeDigitsSpacing;
    public char pointFormat = '.';
    [Header("Animation")]
    [Min(0)] public float incrementSpeed = 10f;
    [Min(0)] public float maxIncrementTime = 10f;
    public Color idleColor = Color.grey;
    public Color animatedColor = Color.white;
    public float colorFadeInSpeed = 1000f;
    public float colorFadeOutSpeed = 1f;
    [Header("Input")]
    public float value;

    public float CurrentDisplayValue { get; private set; }
    public string CurrentDisplayString => GetValueString(CurrentDisplayValue);
    public bool IsMoving { get; private set; }

    public void Update()
    {
        if (Application.isPlaying)
        {
            ColorAnimationUpdate();
            IncrementAnimationUpdate();
        }
        else
        {
            SetValueImmediate(value, playColorAnimation:false);
        }
    }

    private void OnEnable()
    {
        if (textRenderer) textRenderer.enabled = true;
    }

    private void OnDisable()
    {
        if (textRenderer) textRenderer.enabled = false;
        SetValueImmediate(0f, playColorAnimation:false);
    }

    private void IncrementAnimationUpdate()
    {
        IsMoving = (CurrentDisplayValue != value);
        // No Animation
        if (incrementSpeed <= 0f)
        {
            SetValueImmediate(playColorAnimation: true);
            return;
        }
        // Value animation
        if (IsMoving)
        {
            float estimatedTime = Mathf.Abs(CurrentDisplayValue - value) / incrementSpeed;
            if (estimatedTime <= maxIncrementTime) CurrentDisplayValue = Mathf.MoveTowards(CurrentDisplayValue, value, incrementSpeed * Time.deltaTime);
            else CurrentDisplayValue = Mathf.MoveTowards(CurrentDisplayValue, value, estimatedTime / Time.deltaTime);
        }
        // Display value
        if (textRenderer) textRenderer.text = CurrentDisplayString;
    }

    public void SetValue(int value) => SetValue((float)value);

    public void SetValue(float value)
    {
        this.value = value;
        IsMoving = true;
    }

    public void SetValueImmediate(bool playColorAnimation = false)
    {
        CurrentDisplayValue = value;
        IsMoving = false;
        if (textRenderer)
        {
            textRenderer.text = CurrentDisplayString;
            textRenderer.color = playColorAnimation ? animatedColor : idleColor;
        }
    }

    public void SetValueImmediate(float setValue, bool playColorAnimation = false)
    {
        value = setValue;
        SetValueImmediate(playColorAnimation);
    }

    public string GetValueString(float value)
    {
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
        // Return string
        return valueString;
    }

    private void ColorAnimationUpdate()
    {
        if (textRenderer == null) return;
        if (IsMoving) LerpColor(animatedColor, colorFadeInSpeed);
        else LerpColor(idleColor, colorFadeOutSpeed);
    }

    public void SetColor(Color c)
    {
        if (textRenderer == null) return;
        textRenderer.color = c;
    }

    private void LerpColor(Color c, float speed)
    {
        if (textRenderer == null) return;
        if (speed <= 0f) textRenderer.color = c;
        else textRenderer.color = Color.Lerp(textRenderer.color, c, speed * Time.deltaTime);
    }
}
