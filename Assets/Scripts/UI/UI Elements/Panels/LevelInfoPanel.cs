using TMPro;
using UnityEngine;

[ExecuteAlways]
public class LevelInfoPanel : InfoPanel
{
    public TMP_Text overlay;
    public LevelProgress levelInfo;
    public string composerLabel = "Compositeur:";
    public string objectiveLabel = "Objectifs:";

    protected override void OnEnable()
    {
        base.OnEnable();
        if (overlay != null) overlay.enabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (overlay != null) overlay.enabled = false;
    }

    protected override void Update()
    {
        if (textOverride == null || textOverride == "")
            UpdateText();
        else
        {
            textField?.SetText(textOverride);
            overlay?.SetText("");
        }
    }

    public override void UpdateText()
    {
        string text = "";
        string overlayText = "";
        SheetMusic music = levelInfo.levelAsset?.music;
        // Music info
        if (music != null)
        {
            text += music.title + "\n";
            overlayText += "\n";
            if (music.subtitle != "")
            {
                text += music.subtitle + "\n";
                overlayText += "\n";
            }
            if (music.composer != "")
            {
                text += composerLabel + music.composer + "\n";
                overlayText += "\n";
            }
            text += "\n";
            overlayText += "\n";
        }
        // Objectives
        int objectiveCount = levelInfo.ObjectiveCount;
        if (objectiveCount > 0)
        {
            text += objectiveLabel + "\n\n";
            overlayText += "\n\n";
            string[] objectives = levelInfo.ObjectiveLongNames;
            bool[] completed = levelInfo.Checklist;
            for (int i = 0; i < objectiveCount; i++)
            {
                string line = "- " + objectives[i] + "\n";
                text += line;
                overlayText += completed[i] ? line : "\n";
            }
        }
        // Set field
        if (textField != null) textField.text = text;
        if (overlay != null) overlay.text = overlayText;
    }
}
