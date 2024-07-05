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
        if (Application.isPlaying)
        {
            //if (levelInfoPanel != null) levelInfoPanel.levelInfo = new(null);
            ModifierSelectionButton[] buttons = GetComponentsInChildren<ModifierSelectionButton>(true);
            if (buttons != null) foreach (ModifierSelectionButton b in buttons) b.AddListeners(ShowModifierInfo, SelectModifier);
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            ModifierSelectionButton[] buttons = GetComponentsInChildren<ModifierSelectionButton>(true);
            if (buttons != null) foreach (ModifierSelectionButton b in buttons) b.RemoveListeners(ShowModifierInfo, SelectModifier);
        }
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
        TromboneBuildModifier[] activeMods = modifierStack?.mods != null ? modifierStack.mods.ToArray() : new TromboneBuildModifier[0];
        // Update UI
        if (modifierGrid != null)
        {
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
        }
    }

    private void ShowModifierInfo(TromboneBuildModifier modifier, bool show)
    {
    }

    private void SelectModifier(TromboneBuildModifier modifier, bool active)
    {
        if (active) modifierStack?.mods?.Add(modifier);
        else modifierStack?.mods?.Remove(modifier);
        modifierStack?.ApplyStack();
    }
}
