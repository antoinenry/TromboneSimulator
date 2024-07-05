using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelInfoPanel : MonoBehaviour
{
    public TMP_Text textField;
    public TMP_Text overlay;
    public Image background;
    public LevelProgress levelInfo;

    private void OnEnable()
    {
        if (textField != null) textField.enabled = true;
        if (overlay != null) overlay.enabled = true;
        if (background != null) background.enabled = true;
    }

    private void OnDisable()
    {
        if (textField != null) textField.enabled = false;
        if (overlay != null) overlay.enabled = false;
        if (background != null) background.enabled = false;
    }

    private void Update()
    {
        SetText();
    }

    private void SetText()
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
