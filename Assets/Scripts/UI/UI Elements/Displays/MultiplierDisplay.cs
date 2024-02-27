using UnityEngine;
using TMPro;

[ExecuteAlways]
public class MultiplierDisplay : MonoBehaviour
{
    public enum ValueType { COUNTER, RATIO }


    public TextMeshProUGUI textComponent;
    public string label = "multiplier";
    public float value = 1f;
    public ValueType type;
    public float lowerValue = 0f;
    public Color lowerColor = Color.gray;
    public float higherValue = 0f;
    public Color higherColor = Color.white;

    private void Reset()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (textComponent != null)
        {
            string valueString = value.ToString();
            switch (type)
            {
                case ValueType.COUNTER:
                    if (value >= lowerValue) valueString = "x" + Mathf.FloorToInt(value).ToString();
                    else valueString = "";
                    break;
                case ValueType.RATIO:
                    if (value >= lowerValue) valueString = "x" + Mathf.FloorToInt(value).ToString() + "." + Mathf.FloorToInt((value * 10f) % 10f).ToString("0");
                    else valueString = "";
                    break;
            }
            textComponent.text = label + valueString;
            if (higherValue != lowerValue)
                textComponent.color = Color.Lerp(lowerColor, higherColor, (value - lowerValue) / (higherValue - lowerValue));
            else
                textComponent.color = lowerColor;
        }
    }
}
