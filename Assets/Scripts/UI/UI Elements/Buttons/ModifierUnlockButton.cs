using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class ModifierUnlockButton : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text nameField;
    public TMP_Text nameFieldShadow;
    public TMP_Text descriptionField;
    public Image icon;
    [Header("Configuration")]
    public TromboneBuildModifier modifier;
    public bool showStats;
    [Header("Events")]
    public UnityEvent<TromboneBuildModifier> onClick;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button) button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (button) button.onClick.RemoveListener(OnClick);
    }
    private void Update()
    {
        if (modifier)
        {
            if (nameField)
            {
                nameField.text = modifier.modName;
                nameField.color = modifier.uiColor;
            }
            if (nameFieldShadow)
            {
                nameFieldShadow.text = modifier.modName;
            }
            if (descriptionField)
            {
                descriptionField.text = (showStats && modifier.StatDescription != null) ? modifier.StatDescription : modifier.description;
            }
            if (icon)
            {
                icon.sprite = modifier.icon;
                icon.color = modifier.IconColor;
            }
        }
        else
        {
            nameField?.SetText("");
            nameFieldShadow?.SetText("");
            descriptionField?.SetText("");
            if (icon) icon.sprite = null;
        }
    }

    public void ShowStats() => showStats = true;
    public void HideStats() => showStats = false;

    private void OnClick() => onClick.Invoke(modifier);
}
