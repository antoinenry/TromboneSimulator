using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SliderColorizer : MonoBehaviour
{
    [Header("Components")]
    public Slider slider;
    public Image[] coloredComponents;
    [Header("Colors")]
    public bool onIdle = true;
    public Color idleColor = Color.white;
    public bool onIncrease = true;
    public Color increaseColor = Color.green;
    public bool onDecrease = true;
    public Color decreaseColor = Color.red;

    private float sliderValue;

    private void OnEnable()
    {
        if (slider) sliderValue = slider.value;
    }

    private void Update()
    {
        if (slider == null) return;
        float value = slider.value;
        if (onIdle && value == sliderValue) Colorize(idleColor);
        else if (onIncrease && value > sliderValue) Colorize(increaseColor);
        else if (onDecrease && value < sliderValue) Colorize(decreaseColor);
        sliderValue = value;
    }

    private void Colorize(Color c)
    {
        if (coloredComponents == null) return;
        foreach (Image component in coloredComponents)
            if (component != null) component.color = c;
    }
}
