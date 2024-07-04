using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelSelectionButton : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text titleField;
    public TMP_Text durationField;
    public TMP_Text progressField;

    public UnityEvent<Level> onClick;

    private Level levelAsset;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button?.onClick?.AddListener(OnClick);
    }

    private void OnDisable()
    {
        button?.onClick?.RemoveListener(OnClick);
    }

    public void SetLevel(Level l, int completedObjectives, int totalObjectives)
    {
        levelAsset = l;
        if (l == null)
        {
            SetTitle(null);
            SetDuration(0f);
            SetProgress(0, 0);
            return;
        }
        SetTitle(l.name);
        SetDuration(l.MusicDuration);
        SetProgress(completedObjectives, totalObjectives);
    }

    public void SetLevel(Level l)
    {
        if (l == null) SetLevel(l, 0, 0);
        l.TryGetCurrentProgress(out int completed, out int total);
        SetLevel(l, completed, total);
    }

    public void SetLevel(LevelProgress l)
        => SetLevel(l.levelAsset, l.CompletedObjectivesCount, l.ObjectiveCount);

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

    private void OnClick() => onClick.Invoke(levelAsset);
}
