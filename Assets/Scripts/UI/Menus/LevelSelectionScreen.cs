using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class LevelSelectionScreen : MenuUI
{
    [Header("UI Components")]
    public IndexedButton levelButtonPrefab;
    public RectTransform levelListUI;
    public TMP_InputField passwordInput;
    public Button backButton;
    [Header("Contents")]
    public string lockedLevelText = "?";
    public string[] levelNames;
    public int unlockedLevelCount;
    [Header("Events")]
    public UnityEvent<Level> onSelectLevel;

    public override void ShowUI()
    {
        base.ShowUI();
        UpdateLevelList();
        if (Application.isPlaying)
        {
            IndexedButton[] buttons = GetComponentsInChildren<IndexedButton>(true);
            if (buttons != null)
                foreach (IndexedButton b in buttons) b.onClick.AddListener(SelectLevel);
            passwordInput.onSubmit.AddListener(EnterPassword);
            backButton.onClick.AddListener(GoBack);
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            IndexedButton[] buttons = GetComponentsInChildren<IndexedButton>(true);
            if (buttons != null)
                foreach (IndexedButton b in buttons) b.onClick.RemoveListener(SelectLevel);
            passwordInput.onSubmit.RemoveListener(EnterPassword);
            backButton.onClick.RemoveListener(GoBack);
        }
    }

    public void UpdateLevelList()
    {
        // Get level list
        if (GameContentLibrary.Current != null)
        {
            levelNames = GameContentLibrary.Current.GetLevelNames();
        }
        // Update UI
        if (levelListUI != null)
        {
            IndexedButton[] buttons = GetComponentsInChildren<IndexedButton>(true);
            foreach(IndexedButton b in buttons)
            {
                if (Application.isPlaying) Destroy(b.gameObject);
                else DestroyImmediate(b.gameObject);
            }
            if (levelButtonPrefab != null)
            {
                int levelCount = levelNames != null ? levelNames.Length : 0;
                for (int i = 0; i < levelCount; i++)
                {
                    IndexedButton b = Instantiate(levelButtonPrefab, levelListUI);
                    b.index = i;
                    //b.button.image.color = levelCollection.levels[i].color;
                    if (i < unlockedLevelCount)
                    {
                        b.text = levelNames[i];
                        b.button.interactable = true;
                    }
                    else
                    {
                        b.text = lockedLevelText;
                        b.button.interactable = false;
                    }
                    // Ensure that buttons are displayed correctly in the same frame
                    b.Update();
                }    
            }
        }
    }

    private void SelectLevel(int index)
    {
        Level getLevel = GameContentLibrary.Current != null ? GameContentLibrary.Current.levels[index] : null;
        HideUI();
        onSelectLevel.Invoke(getLevel);
    }

    private void EnterPassword(string word)
    {
        //gameState.SubmitPassword(word);
        UpdateLevelList();
    }
}
