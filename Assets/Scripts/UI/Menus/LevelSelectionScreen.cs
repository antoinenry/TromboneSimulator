using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelSelectionScreen : MenuUI
{
    [Header("UI Components")]
    public LevelSelectionButton buttonPrefab;
    public TMP_Text lockedPrefab;
    public LayoutGroupScroller levelList;
    public LevelInfoPanel levelInfoPanel;
    public TMP_Text playerXPField;
    [Header("Contents")]
    public LevelProgress[] levels;
    public int selectedLevelIndex;
    public bool clearSelectionOnShow;
    public int playerXP;
    public string playerXPPrefix = "Niveau";
    public string defaultInfoPanelContent;

    private List<Level> newLevels; 

    private void OnEnable()
    {
        GameProgress.Current?.onUnlockContent?.AddListener(OnUnlockContent);
    }

    private void OnDisable()
    {
        GameProgress.Current?.onUnlockContent?.RemoveListener(OnUnlockContent);
    }

    public override void ShowUI()
    {
        GameProgress.Current?.Update();
        base.ShowUI();
        UpdatePlayerXP();
        UpdateLevelList();
        if (Application.isPlaying)
        {
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.AddListeners(OnLevelButtonHighlighted, OnLevelButtonPressed);
        }
        if (levelInfoPanel != null)
        {
            levelInfoPanel.levelInfo = new(null);
            levelInfoPanel.textOverride = defaultInfoPanelContent;
        }
        if (clearSelectionOnShow) selectedLevelIndex = -1;
        SelectLevel(selectedLevelIndex);
        ShowLevelInfo(selectedLevelIndex);
        newLevels?.Clear();
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

    public void UpdatePlayerXP()
    {
        GameProgress progress = GameProgress.Current;
        progress?.GetObjectiveCount(out playerXP);
        playerXPField?.SetText(playerXPPrefix + playerXP.ToString());
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
                LevelSelectionButton unlockedInstance;
                TMP_Text lockedInstance;
                if (unlocked)
                {
                    if (buttonPrefab == null) continue;
                    unlockedInstance = Instantiate(buttonPrefab);
                    unlockedInstance.SetLevel(level);
                    unlockedInstance.MarkAsNew(newLevels != null && newLevels.Contains(level.levelAsset));
                    levelList.Add(unlockedInstance.gameObject);
                }
                else
                {
                    if (lockedPrefab == null) continue;
                    lockedInstance = Instantiate(lockedPrefab);
                    lockedInstance.SetText(level.levelAsset != null ? (playerXPPrefix + level.levelAsset.unlockTier) : "");
                    levelList.Add(lockedInstance.gameObject);
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

    private void OnLevelButtonPressed(LevelProgress levelInfo)
    {
        SelectLevel(levelInfo.levelAsset);
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
        if (levelInfoPanel == null) return;
        levelInfoPanel.textOverride = null;
        if (show) levelInfoPanel.levelInfo = info;
        else if (levelInfoPanel.levelInfo.levelAsset == info.levelAsset)
        {
            if (defaultInfoPanelContent == "" || defaultInfoPanelContent == null)
                levelInfoPanel.levelInfo = selectedLevelIndex != -1 ? levels[selectedLevelIndex] : new();
            else
                levelInfoPanel.textOverride = defaultInfoPanelContent;
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

    private void OnUnlockContent(ScriptableObject contentAsset)
    {
        if (contentAsset == null || contentAsset is Level == false) return;
        if (newLevels == null) newLevels = new List<Level>();
        newLevels.Add(contentAsset as Level);
    }
}
