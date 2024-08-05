using UnityEngine;
using TMPro;

public class LevelSelectionButton : SelectionButton<LevelProgress>
{
    [Header("Components")]
    public TMP_Text titleField;
    public TMP_Text durationField;
    public TMP_Text progressField;
    public GameObject newNotification;
    [Header("Content")]
    public LevelProgress levelInfo;

    public override LevelProgress Selection { get => levelInfo; set => levelInfo = value; }

    public void SetLevel(LevelProgress l)
    {
        levelInfo = l;
        if (l.levelAsset == null)
        {
            SetTitle(null);
            SetDuration(0f);
            SetProgress(0, 0);
            return;
        }
        SetTitle(l.levelAsset.levelName);
        SetDuration(l.levelAsset.MusicDuration);
        SetProgress(l.CompletedObjectivesCount, l.ObjectiveCount);
    }

    public void SetTitle(string title)
    {
        if (titleField == null) return;
        if (title == null)
        {
            titleField.text = "";
            return;
        }
        titleField.text = title;
    }

    public void SetDuration(float durationSeconds)
    {
        if (durationField == null) return;
        if (durationSeconds == 0f)
        {
            durationField.text = "";
            return;
        }
        int minutes = Mathf.FloorToInt(durationSeconds / 60f);
        int seconds = Mathf.FloorToInt(durationSeconds % 60f);
       durationField.text = minutes.ToString() + ":" + seconds.ToString("00");
    }

    public void SetProgress(int completedObjectives, int totalObjectives)
    {
        if (progressField == null) return;
        if (totalObjectives == 0)
        {
            progressField.text = "";
            return;
        }
        int percentage = Mathf.FloorToInt(100f * completedObjectives / totalObjectives);
        progressField.text = percentage + "%";
    }

    public void MarkAsNew(bool isNew) => newNotification?.SetActive(isNew);

    public Level LevelAsset => levelInfo.levelAsset;
}
