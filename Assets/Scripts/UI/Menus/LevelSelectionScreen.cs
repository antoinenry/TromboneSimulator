using System;
using UnityEngine;
using UnityEngine.Events;

public class LevelSelectionScreen : MenuUI
{
    [Header("UI Components")]
    public LevelSelectionButton buttonPrefab;
    public VerticalScrollList levelList;
    [Header("Contents")]
    public string lockedLevelText = "?";
    public LevelProgress[] levels;
    [Header("Events")]
    public UnityEvent<Level> onSelectLevel;

    public override void ShowUI()
    {
        base.ShowUI();
        UpdateLevelList();
        if (Application.isPlaying)
        {
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.onClick.AddListener(SelectLevel);
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            LevelSelectionButton[] buttons = GetComponentsInChildren<LevelSelectionButton>(true);
            if (buttons != null) foreach (LevelSelectionButton b in buttons) b.onClick.RemoveListener(SelectLevel);
        }
    }

    public void UpdateLevelList()
    {
        if (GameProgress.Current == null) return;
        // Get level list
        int levelCount = GameProgress.Current.GetLevelCount();
        levels = new LevelProgress[levelCount];
        Array.Copy(GameProgress.Current.levelProgress, levels, levelCount);
        // Update UI
        if (levelList != null)
        {
            levelList.Clear();
            foreach(LevelProgress level in levels)
            {
                if (buttonPrefab == null) break;
                LevelSelectionButton b = Instantiate(buttonPrefab);
                b.SetLevel(level);
                levelList.Add(b.gameObject);
            }
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
