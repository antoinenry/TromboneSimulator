using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ModifierSelectionScreen : MenuUI
{
    [Header("Components")]
    public TromboneBuildStack modifierStack;
    public ModifierSelectionToggle modifierTogglePrefab;
    public LayoutGroupScroller modifierGrid;
    public ModifierInfoPanel modifierInfoPanel;
    public BuildStackInfoPanel stackInfoPanel;
    public TMP_Text levelNameField;
    [Header("Contents")]
    public TromboneBuildModifier[] modifiers;
    [Header("Events")]
    public UnityEvent<TromboneBuildModifier> onSelectModifier;

    public ObjectMethodCaller caller = new("ResetModifierGrid");


    public override void ShowUI()
    {
        base.ShowUI();
        ResetModifierGrid();
        if (Application.isPlaying)
        {
            AddButtonGridListeners();
            DisplayLevelName();
            LoadingScreen loadingScreen = Get<LoadingScreen>();
            if (loadingScreen) loadingScreen.showAsPanel = true;
        }
        ShowModifierInfo(null, true);
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying) RemoveButtonGridListeners();
    }

    protected override void Update()
    {
        base.Update();
        UpdateModifierGrid();
    }

    public void UpdateModifierGrid()
    {
        GameProgress progress = GameProgress.Current;
        if (progress?.contentLocks == null) return;
        // Get modifier list
        modifiers = Array.ConvertAll(
            Array.FindAll(progress.contentLocks, c => c.contentAsset != null && c.contentAsset is TromboneBuildModifier && progress.CanUnlock(c.contentAsset)),
            c => c.contentAsset as TromboneBuildModifier);
        // Update button grid
        if (modifierGrid != null)
        {
            ModifierSelectionToggle[] buttons = modifierGrid.GetComponentsInChildren<ModifierSelectionToggle>(true);
            // Check if there's a button for each mod, if not correct it
            if (modifiers.Length != buttons.Length)
            {
                ResetModifierGrid();
                return;
            }
            else for (int i = 0, iend = modifiers.Length; i < iend; i++)
                    if (modifiers[i] != buttons[i].modifierAsset)
                    {
                        ResetModifierGrid();
                        return;
                    }
            // Update active/inactive/locked states
            TromboneBuildModifier[] activeMods = modifierStack != null ? modifierStack.Modifiers : new TromboneBuildModifier[0];
            foreach (ModifierSelectionToggle b in buttons)
            {
                TromboneBuildModifier modifierAsset = b?.modifierAsset;
                if (modifierAsset == null) continue;
                b.isLocked = progress.IsUnlocked(modifierAsset) == false;
                b.SetToggle(Array.IndexOf(activeMods, modifierAsset) != -1);
            }
        }
    }

    private void ResetModifierGrid()
    {
        // Get active modifiers
        ModifierSelectionToggle[] buttons = modifierGrid?.GetComponentsInChildren<ModifierSelectionToggle>(true);
        RemoveButtonGridListeners(buttons);
        modifierGrid.Clear();
        foreach (TromboneBuildModifier mod in modifiers)
        {
            if (modifierTogglePrefab == null) break;
            ModifierSelectionToggle b;
            b = Instantiate(modifierTogglePrefab);
            b.SetModifier(mod);
            modifierGrid.Add(b.gameObject);
        }
        AddButtonGridListeners(buttons);
        ShowModifierInfo(null, true);
    }

    private void AddButtonGridListeners(ModifierSelectionToggle[] buttons)
    {
        if (buttons != null) foreach (ModifierSelectionToggle b in buttons) b.AddListeners(ShowModifierInfo, SelectModifier);
    }

    private void AddButtonGridListeners() 
        => AddButtonGridListeners(modifierGrid?.GetComponentsInChildren<ModifierSelectionToggle>(true));

    private void RemoveButtonGridListeners(ModifierSelectionToggle[] buttons)
    {
        if (buttons != null) foreach (ModifierSelectionToggle b in buttons) b.RemoveListeners(ShowModifierInfo, SelectModifier);
    }

    private void RemoveButtonGridListeners() 
        => RemoveButtonGridListeners(modifierGrid?.GetComponentsInChildren<ModifierSelectionToggle>(true));

    private void ShowModifierInfo(TromboneBuildModifier modifier, bool show)
    {
        // Show stack info when no modifier is selected
        bool showModifierInfo = show || modifierInfoPanel.modifierAsset != modifier;
        // Modifier info
        if (showModifierInfo)
        {
            if (stackInfoPanel) stackInfoPanel.enabled = false;
            if (modifierInfoPanel)
            {
                modifierInfoPanel.enabled = true;
                if (show) modifierInfoPanel.modifierAsset = modifier;
                else if (modifierInfoPanel.modifierAsset == modifier) modifierInfoPanel.modifierAsset = null;
            }
        }
        // Stack info
        else
        {
            if (modifierInfoPanel) modifierInfoPanel.enabled = false;
            if (stackInfoPanel) stackInfoPanel.enabled = true;
        }
    }

    private void SelectModifier(TromboneBuildModifier modifier, bool active)
    {
        if (modifierStack == null) return;
        if (active) modifierStack.TryAddModifier(modifier);
        else modifierStack.TryRemoveModifier(modifier);
        modifierStack.ApplyStack();
        UpdateModifierGrid();
        onSelectModifier.Invoke(modifier);
    }

    private void DisplayLevelName()
    {
        if (levelNameField == null) return;
        Level l = Get<LevelSelectionScreen>()?.GetSelectedLevel();
        levelNameField.text = l != null ? l.levelName : "";
    }
}
