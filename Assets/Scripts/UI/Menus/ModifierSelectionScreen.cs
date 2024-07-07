using System;
using UnityEngine;
using UnityEngine.Events;

public class ModifierSelectionScreen : MenuUI
{
    public TromboneBuildStack modifierStack;
    [Header("UI Components")]
    public ModifierSelectionButton buttonPrefab;
    public LayoutGroupScroller modifierGrid;
    public LevelInfoPanel modifierInfoPanel;
    [Header("Contents")]
    public TromboneBuildModifier[] modifiers;
    [Header("Events")]
    public UnityEvent<TromboneBuildModifier> onSelectModifier;

    public override void ShowUI()
    {
        base.ShowUI();
        UpdateModifierGrid();
        if (Application.isPlaying) AddButtonGridListeners();
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying) RemoveButtonGridListeners();
    }

    public void UpdateModifierGrid()
    {
        GameProgress progress = GameProgress.Current;
        if (progress?.contentLocks == null) return;
        // Get modifier list
        modifiers = Array.ConvertAll(
            Array.FindAll(progress.contentLocks, c => c.locked == false && c.contentAsset is TromboneBuildModifier),
            c => c.contentAsset as TromboneBuildModifier);
        int modifierCount = progress.GetLevelCount();
        // Get active modifiers
        TromboneBuildModifier[] activeMods = modifierStack != null ? modifierStack.Modifiers : new TromboneBuildModifier[0];
        // Update button grid
        if (modifierGrid != null)
        {
            ModifierSelectionButton[] buttons = modifierGrid.GetComponentsInChildren<ModifierSelectionButton>(true);
            // Check if there's a button for each mod, if not correct it
            if (Array.FindIndex(modifiers, mod => Array.FindIndex(buttons, b => b.modifierAsset == mod) == -1) !=-1)
                ResetButtonGrid(buttons, activeMods);
            // Update active/inactive states
            foreach(ModifierSelectionButton b in buttons) b.active = Array.IndexOf(activeMods, b.modifierAsset) != -1;
        }
    }

    private void ResetButtonGrid(ModifierSelectionButton[] buttons, TromboneBuildModifier[] activeMods)
    {
        RemoveButtonGridListeners(buttons);
        modifierGrid.Clear();
        foreach (TromboneBuildModifier mod in modifiers)
        {
            if (buttonPrefab == null) break;
            ModifierSelectionButton b;
            b = Instantiate(buttonPrefab);
            b.SetModifier(mod);
            b.active = Array.IndexOf(activeMods, mod) != -1;
            modifierGrid.Add(b.gameObject);
        }
        AddButtonGridListeners(buttons);
    }

    private void AddButtonGridListeners(ModifierSelectionButton[] buttons)
    {
        if (buttons != null) foreach (ModifierSelectionButton b in buttons) b.AddListeners(ShowModifierInfo, SelectModifier);
    }

    private void AddButtonGridListeners() 
        => AddButtonGridListeners(modifierGrid?.GetComponentsInChildren<ModifierSelectionButton>(true));

    private void RemoveButtonGridListeners(ModifierSelectionButton[] buttons)
    {
        if (buttons != null) foreach (ModifierSelectionButton b in buttons) b.RemoveListeners(ShowModifierInfo, SelectModifier);
    }

    private void RemoveButtonGridListeners() 
        => RemoveButtonGridListeners(modifierGrid?.GetComponentsInChildren<ModifierSelectionButton>(true));

    private void ShowModifierInfo(TromboneBuildModifier modifier, bool show)
    {
    }

    private void SelectModifier(TromboneBuildModifier modifier, bool active)
    {
        if (modifierStack == null) return;
        if (active) modifierStack.TryAddModifier(modifier);
        else modifierStack.TryRemoveModifier(modifier);
        modifierStack.ApplyStack();
    }
}
