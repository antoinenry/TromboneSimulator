using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class InfoPanel : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text textField;
    public Image background;
    [Header("Content")]
    public string textOverride = "";

    protected virtual void OnEnable()
    {
        if (textField != null) textField.enabled = true;
        if (background != null) background.enabled = true;
    }

    protected virtual void OnDisable()
    {
        if (textField != null) textField.enabled = false;
        if (background != null) background.enabled = false;
    }

    protected virtual void Update()
    {
        if (textOverride == null || textOverride == "")
            UpdateText();
        else
            textField?.SetText(textOverride);
    }

    public abstract void UpdateText();
}
