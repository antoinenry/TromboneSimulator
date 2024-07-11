using TMPro;
using UnityEngine;

[ExecuteAlways]
public class LevelInfoPanel : InfoPanel
{
    public TMP_Text overlay;
    public LevelProgress levelInfo;

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

    public override void UpdateText()
    {
        string text = "";
        string overlayText = "";
        SheetMusic music = levelInfo.levelAsset?.music;
        // Music info
        if (music != null)
        {
            text += music.title + "\n"
                + music.composer + "\n"
                + music.subtitle + "\n\n";
            overlayText += "\n\n\n\n";
        }
        // Objectives
        int objectiveCount = levelInfo.ObjectiveCount;
        if (objectiveCount > 0)
        {
            string[] objectives = levelInfo.ObjectiveNames;
            bool[] completed = levelInfo.checkList;
            for (int i = 0; i < objectiveCount; i++)
            {
                text += objectives[i] + "\n";
                if (completed[i]) overlayText += objectives[i];
                overlayText += "\n";
            }
        }
        // Set field
        if (textField != null) textField.text = text;
        if (overlay != null) overlay.text = overlayText;
    }
}
