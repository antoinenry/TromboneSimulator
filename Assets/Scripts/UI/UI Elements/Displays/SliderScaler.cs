using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SliderScaler : MonoBehaviour
{
    public float maxValue = 10f;
    public float value = 0f;
    public float valueScale = 1f;

    private Slider slider;
    private RectTransform rectTransform;

    private void Awake()
    {
        FindComponents();
    }

    private void LateUpdate()
    {
        if (FindComponents() == false) return;
        UpdateValue();
        UpdateScale();
    }

    private bool FindComponents()
    {
        if (slider != null && rectTransform != null) return true;
        slider = GetComponent<Slider>();
        rectTransform = GetComponent<RectTransform>();
        return rectTransform != null && slider != null;
    }

    private void UpdateScale()
    {
        if (rectTransform == null) return;
        Vector2 size = rectTransform.sizeDelta;
        if (slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft)
            size.x = valueScale * maxValue;
        else
            size.y = valueScale * maxValue;
        rectTransform.sizeDelta = size;
    }

    private void UpdateValue()
    {
        if (slider == null) return;
        value = Mathf.Clamp(value, 0f, maxValue);
        slider.maxValue = maxValue * valueScale;
        slider.value = value * valueScale;
    }

    public void SetValueAndMax(int value, int maxValue)
    {
        this.value = value;
        this.maxValue = maxValue;
        UpdateScale();
        UpdateValue();
    }
}