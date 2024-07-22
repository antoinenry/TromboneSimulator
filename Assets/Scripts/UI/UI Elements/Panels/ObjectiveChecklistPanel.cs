using TMPro;
using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class ObjectiveChecklistPanel : InfoPanel
{
    public TMP_Text overlay;
    public LevelProgress levelInfo;
    public float secondsBetweenLines = .75f;

    private string baseText;
    private string overlayText;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (overlay != null) overlay.enabled = true;
        StartCoroutine(LineByLineCoroutine());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (overlay != null) overlay.enabled = false;
        StopAllCoroutines();
    }

    public override void UpdateText()
    {
        baseText = "";
        overlayText = "";
        // Objectives
        int objectiveCount = levelInfo.ObjectiveCount;
        if (objectiveCount > 0)
        {
            string[] objectives = levelInfo.ObjectiveNames;
            bool[] completed = levelInfo.checkList;
            int checkListLength = completed != null ? completed.Length : 0;
            for (int i = 0; i < objectiveCount; i++)
            {
                baseText += objectives[i] + "\n";
                if (i < checkListLength && completed[i]) overlayText += objectives[i];
                overlayText += "\n";
            }
        }
        // Instant display in editor
        if (Application.isPlaying == false)
        {
            if (textField != null) textField.text = baseText;
            if (overlay != null) overlay.text = overlayText;
        }
    }

    private IEnumerator LineByLineCoroutine()
    {
        int lineCounter = 0;
        string baseTextLines = "";
        string overlayTextLines = "";
        while (baseTextLines != baseText && overlayTextLines != overlayText)
        {
            baseTextLines = GetLines(baseText, lineCounter);
            overlayTextLines = GetLines(overlayText, lineCounter);
            textField.text = baseTextLines;
            overlay.text = overlayTextLines;
            lineCounter++;
            yield return new WaitForSeconds(secondsBetweenLines);
        }
    }

    private string GetLines(string s, int lineCount)
    {
        int stringLength = s != null ? s.Length : 0;
        if (stringLength == 0) return "";
        int charCount = 0;
        for (int line = 0; line < lineCount; line++)
        {
            charCount = s.IndexOf('\n', charCount) + 1;
            if (charCount == -1 || charCount >= stringLength) return s;
        }
        return s.Substring(0, charCount);
    }
}
