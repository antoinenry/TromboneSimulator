using UnityEngine;

public class FadingValueDisplay : FadingTextDisplay
{
    public string prefix = "+";
    public string suffix = "pts";
    public string format;

    public void ShowValue(int value) => ShowText(prefix + value.ToString() + suffix);

    public void SetValue(float value) => ShowText(prefix + value.ToString(format) + suffix);
}