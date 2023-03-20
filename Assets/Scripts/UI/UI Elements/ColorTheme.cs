using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class ColorTheme : MonoBehaviour
{
    public Color color = Color.white;
    public SpriteRenderer[] spritesToColor;
    public TextMeshProUGUI[] textsToColor;

    private void Update()
    {
        SetColors();
    }

    public void SetColors()
    {
        foreach (SpriteRenderer s in spritesToColor)
            if (s != null)
            {
                Color c = color;
                c.a = s.color.a;
                s.color = c;
            }
        foreach (TextMeshProUGUI t in textsToColor)
            if (t != null)
            {
                Color c = color;
                c.a = t.color.a;
                t.color = c;
            }
    }
}
