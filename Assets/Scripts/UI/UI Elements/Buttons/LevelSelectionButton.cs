using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class LevelSelectionButton : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text titleField;
    public TMP_Text durationField;
    public TMP_Text progressField;

    public UnityEvent<LevelProgress,bool> onSelect;
    public UnityEvent<Level> onClick;

    private LevelProgress levelInfo;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void AddListeners(UnityAction<LevelProgress,bool> onSelectAction, UnityAction<Level> onClickAction)
    {
        if (onSelectAction != null) onSelect.AddListener(onSelectAction);
        if (onClickAction != null) onClick.AddListener(onClickAction);
    }

    public void RemoveListeners(UnityAction<LevelProgress,bool> onSelectAction, UnityAction<Level> onClickAction)
    {
        if (onSelectAction != null) onSelect.RemoveListener(onSelectAction);
        if (onClickAction != null) onClick.RemoveListener(onClickAction);
    }

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
        SetTitle(l.levelAsset.name);
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

    public void OnSelect() => onSelect.Invoke(levelInfo, true);

    public void OnUnselect() => onSelect.Invoke(levelInfo, false);

    public void OnClick() => onClick.Invoke(levelInfo.levelAsset);
}
