using System;
using UnityEngine;
using UnityEngine.Events;

public class LevelSelectionScreen : MenuUI
{
    [Header("UI Components")]
    public LevelSelectionButton buttonPrefab;
    public GameObject lockedPrefab;
    public LayoutGroupScroller levelList;
    public LevelInfoPanel levelInfoPanel;
    [Header("Contents")]
    public LevelProgress[] levels;
    public int selectedLevelIndex;
    public bool clearSelectionOnShow;

    public override void ShowUI()
    {
        base.ShowUI();
        UpdateLevelList();
        if (Application.isPlaying)
        {
            if (levelInfoPanel != null) levelInfoPanel.levelInfo = new(null);
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.AddListeners(OnLevelButtonHighlighted, OnLevelButtonPressed);
        }
        if (clearSelectionOnShow) selectedLevelIndex = -1;
        SelectLevel(selectedLevelIndex);
        ShowLevelInfo(selectedLevelIndex);
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.RemoveListeners(OnLevelButtonHighlighted, OnLevelButtonPressed);
        }
    }

    public void UpdateLevelList()
    {
        GameProgress progress = GameProgress.Current;
        if (progress == null) return;
        // Get level list
        int levelCount = progress.GetLevelCount();
        levels = new LevelProgress[levelCount];
        Array.Copy(progress.levelProgress, levels, levelCount);
        // Update UI
        if (levelList != null)
        {
            levelList.Clear();
            foreach(LevelProgress level in levels)
            {
                if (buttonPrefab == null) break;
                bool unlocked = progress.IsUnlocked(level.levelAsset);
                LevelSelectionButton b;
                if (unlocked)
                {
                    b = Instantiate(buttonPrefab);
                    b.SetLevel(level);
                    levelList.Add(b.gameObject);
                }
                else
                {
                    levelList.Add(Instantiate(lockedPrefab));
                }
            }
        }
    }

    public Level GetSelectedLevel()
    {
        int levelCount = levels != null ? levels.Length : 0;
        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelCount)
            return levels[selectedLevelIndex].levelAsset;
        else
            return null;
    }

    private void OnLevelButtonHighlighted(LevelProgress levelInfo, bool enter)
    {
        ShowLevelInfo(levelInfo, enter);
    }

    private void OnLevelButtonPressed(Level levelAsset)
    {
        SelectLevel(levelAsset);
        GoTo(Get<ModifierSelectionScreen>());
    }

    private void ShowLevelInfo(int levelIndex)
    {
        int levelCount = levels != null ? levels.Length : 0;
        if (levelIndex >= 0 && levelIndex < levelCount) ShowLevelInfo(levels[levelIndex], true);
        else ShowLevelInfo(new LevelProgress(), true);
    }

    private void ShowLevelInfo(LevelProgress info, bool show)
    {
        if (levelInfoPanel)
        {
            if (show)
                levelInfoPanel.levelInfo = info;
            else if (levelInfoPanel.levelInfo.levelAsset == info.levelAsset)
                levelInfoPanel.levelInfo = selectedLevelIndex != -1 ? levels[selectedLevelIndex] : new();
        }
    }

    private int GetLevelIndex(Level levelAsset) => levels != null ? Array.FindIndex(levels, l => l.levelAsset == levelAsset) : -1;

    private void SelectLevel(int levelIndex)
    {
        int levelCount = levels != null ? levels.Length : 0;
        if (levelIndex >= 0 && levelIndex < levelCount)
            SelectButton(levels[selectedLevelIndex].levelAsset);
        else
            selectedLevelIndex = -1;
    }

    private void SelectLevel(Level levelAsset)
    {
        selectedLevelIndex = GetLevelIndex(levelAsset);
        SelectButton(levelAsset);
    }

    private void SelectButton(Level levelAsset)
    {
        LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
        if (buttons != null) Array.Find(buttons, b => b != null && b.LevelAsset == levelAsset)?.Select();
    }

    private void EnterPassword(string word)
    {
        //gameState.SubmitPassword(word);
        UpdateLevelList();
    }
}
