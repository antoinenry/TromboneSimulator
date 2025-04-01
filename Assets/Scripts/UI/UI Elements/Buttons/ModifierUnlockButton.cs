using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[ExecuteAlways]
public class ModifierUnlockButton : SelectionButton<TromboneBuildModifier>
{
    [Flags]
    public enum ContentToggle { Name = 1, Icon = 2, Description = 4, Stats = 8}

    [Header("Components")]
    public TMP_Text nameField;
    public TMP_Text nameFieldShadow;
    public TMP_Text descriptionField;
    public Image icon;
    [Header("Configuration")]
    public TromboneBuildModifier modifier;
    public ContentToggle unselectedContent = ContentToggle.Name | ContentToggle.Icon;
    public ContentToggle selectedContent = ContentToggle.Name | ContentToggle.Stats;

    public override TromboneBuildModifier Selection { get => modifier; set => modifier = value; }

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
            if (icon)
            {
                icon.sprite = modifier.icon;
                icon.color = modifier.IconColor;
            }
            if (descriptionField)
            {
                descriptionField.text = modifier.StatDescription;
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

    public override void Select()
    {
        base.Select();
        if (nameField == null) return;
        ShowContent(selectedContent);
    }

    public override void Unselect()
    {
        base.Unselect();
        ShowContent(unselectedContent);
    }

    private void ShowContent(ContentToggle visibleContent)
    {
        icon?.gameObject?.SetActive(visibleContent.HasFlag(ContentToggle.Icon));
        nameField?.gameObject?.SetActive(visibleContent.HasFlag(ContentToggle.Name));
        if (descriptionField)
        {
            bool showDescription = visibleContent.HasFlag(ContentToggle.Description),
                showStats = visibleContent.HasFlag(ContentToggle.Stats);
            descriptionField.gameObject.SetActive(showDescription || showStats);
            if (showDescription && showStats) descriptionField.text = modifier.description + "\n" + modifier.StatDescription;
            else if (showDescription) descriptionField.text = modifier.description;
            else if (showStats) descriptionField.text = modifier.StatDescription;
        }
    }
}
