using System;
using UnityEngine;
using UnityEngine.Events;

public class LevelSelectionScreen : MenuUI
{
    [Header("UI Components")]
    public LevelSelectionButton buttonPrefab;
    public GameObject lockedPrefab;
    public VerticalScrollList levelList;
    public LevelInfoPanel levelInfoPanel;
    [Header("Contents")]
    public LevelProgress[] levels;
    [Header("Events")]
    public UnityEvent<Level> onSelectLevel;

    public override void ShowUI()
    {
        base.ShowUI();
        UpdateLevelList();
        if (Application.isPlaying)
        {
            if (levelInfoPanel != null) levelInfoPanel.levelInfo = new(null);
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.AddListeners(ShowLevelInfo, SelectLevel);
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.RemoveListeners(ShowLevelInfo, SelectLevel);
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

    private void ShowLevelInfo(LevelProgress info, bool show)
    {
        if (levelInfoPanel)
        {
            if (show) levelInfoPanel.levelInfo = info;
            else if (levelInfoPanel.levelInfo.levelAsset == info.levelAsset) levelInfoPanel.levelInfo = new(null);
        }
    }

    private void SelectLevel(Level l)
    {
        if (l == null) return;
        HideUI();
        onSelectLevel.Invoke(l);
    }

    private void EnterPassword(string word)
    {
        //gameState.SubmitPassword(word);
        UpdateLevelList();
    }
}
