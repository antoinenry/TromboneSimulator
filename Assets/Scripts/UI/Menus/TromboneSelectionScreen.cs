using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TromboneSelectionScreen : MenuUI
{
    [Header("UI Components")]
    public IndexedButton tromboneButtonPrefab;
    public RectTransform tromboneListUI;
    public Button backButton;
    [Header("Contents")]
    public string[] tromboneNames;
    [Header("Events")]
    public UnityEvent<TromboneBuild> onTromboneSelect;

    public override void ShowUI()
    {
        base.ShowUI();
        UpdateTromboneList();
        if (Application.isPlaying)
        {
            IndexedButton[] buttons = GetComponentsInChildren<IndexedButton>(true);
            if (buttons != null) foreach (IndexedButton b in buttons) b.onClick.AddListener(SelectTrombone);
            backButton.onClick.AddListener(GoBack);
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            IndexedButton[] buttons = GetComponentsInChildren<IndexedButton>(true);
            if (buttons != null) foreach (IndexedButton b in buttons) b.onClick.RemoveListener(SelectTrombone);
            backButton.onClick.RemoveListener(GoBack);
        }
    }

    public void UpdateTromboneList()
    {
        // Get level list
        if (GameContentLibrary.Current != null)
        {
            tromboneNames = GameContentLibrary.Current.GetTromboneNames;
        }
        // Update UI
        if (tromboneListUI != null)
        {
            IndexedButton[] buttons = GetComponentsInChildren<IndexedButton>(true);
            foreach (IndexedButton b in buttons)
            {
                if (Application.isPlaying) Destroy(b.gameObject);
                else DestroyImmediate(b.gameObject);
            }
            if (tromboneButtonPrefab != null)
            {
                int levelCount = tromboneNames != null ? tromboneNames.Length : 0;
                for (int i = 0; i < levelCount; i++)
                {
                    IndexedButton b = Instantiate(tromboneButtonPrefab, tromboneListUI);
                    b.index = i;
                    b.text = tromboneNames[i];
                    // Ensure that buttons are displayed correctly in the same frame
                    b.Update();
                }
            }
        }
    }

    private void SelectTrombone(int index)
    {
        TromboneBuild getTrombone = GameContentLibrary.Current?.trombones[index];
        onTromboneSelect.Invoke(getTrombone);
    }
}
