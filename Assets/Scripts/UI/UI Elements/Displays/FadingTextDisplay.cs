using TMPro;
using UnityEngine;
using System.Collections;

public class FadingTextDisplay : MonoBehaviour
{
    public float fadeInSpeed = 1f;
    public float holdDuration = 1f;
    public float fadeOutSpeed = 1f;

    private TMP_Text textField;
    private float idleAlpha;

    private void Awake()
    {
        textField = GetComponent<TMP_Text>();
        if (textField != null) idleAlpha = textField.color.a;
    }

    public void ShowText(string text)
    {
        if (textField == null) return;
        StopAllCoroutines();
        textField.text = text;
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        Color color = textField.color;
        while (color.a <= idleAlpha)
        {
            color.a = Mathf.MoveTowards(color.a, idleAlpha, fadeInSpeed * Time.deltaTime);
            textField.color = color;
            yield return null;
        }
        if (holdDuration > 0f) yield return new WaitForSeconds(holdDuration);
        while (color.a > 0f)
        {
            color.a = Mathf.MoveTowards(color.a, 0f, fadeInSpeed * Time.deltaTime);
            textField.color = color;
            yield return null;
        }
    }
}
