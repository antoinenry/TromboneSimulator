using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameProgress))]
public class GameProgressInspector : Editor
{
    private GameProgress targetProgress;
    private bool[] levelProgressFoldouts;
    private bool contentLocksFoldout;

    private void OnEnable()
    {
        targetProgress = (GameProgress)target;
    }

    public override void OnInspectorGUI()
    {
        bool isCurrent = CurrentAssetsManager.IsCurrent(targetProgress);
        if (EditorGUILayout.Toggle("IsCurrent", isCurrent))
        {
            if (!isCurrent) CurrentAssetsManager.SetCurrent(targetProgress);
        }
        else if(isCurrent) CurrentAssetsManager.ClearCurrent<GameProgress>();     
        targetProgress.Update();
        LevelProgressGUI();
        EditorGUILayout.Space();
        LockedContentGUI();
        if (GUILayout.Button("Reset")) targetProgress.Reset();
    }

    private void LevelProgressGUI()
    {
        if (targetProgress.levelProgress == null || targetProgress.levelProgress.Length == 0)
        {
            EditorGUILayout.HelpBox("No level found", MessageType.Warning);
            return;
        }
        // Ready foldouts
        int levelCount = targetProgress.levelProgress != null ? targetProgress.levelProgress.Length : 0;
        if (levelProgressFoldouts == null || levelProgressFoldouts.Length != levelCount)
            levelProgressFoldouts = new bool[levelCount];
        // Header
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        // List
        EditorGUI.indentLevel++;
        bool GUIEnabled = GUI.enabled;
        for (int i = 0; i < levelCount; i++)
        {
            LevelProgress l = targetProgress.levelProgress[i];
            levelProgressFoldouts[i] = EditorGUILayout.Foldout(levelProgressFoldouts[i], 
                l.levelAsset.name 
                + " (" + l.CompletedObjectivesCount + "/" + l.ObjectiveCount + ")");
            if (levelProgressFoldouts[i])
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Level", l.levelAsset, typeof(Level), false);
                GUI.enabled = GUIEnabled;
                if (l.ObjectiveCount > 0)
                {
                    string[] objectiveNames = l.ObjectiveNames;
                    bool[] completion = l.checkList;
                    EditorGUILayout.BeginVertical("box");
                    for (int j = 0; j < l.ObjectiveCount; j++)
                    {
                        bool completed = EditorGUILayout.Toggle(objectiveNames[j], completion[j]);
                        if (completed != completion[j]) targetProgress.levelProgress[i].TryCheckObjective(j, completed);
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.HelpBox("No objectives", MessageType.Info);
                }
            }
        }
        EditorGUI.indentLevel--;
    }

    private void LockedContentGUI()
    {
        if (targetProgress.contentLocks == null || targetProgress.contentLocks.Length == 0)
        {
            EditorGUILayout.HelpBox("No content found", MessageType.Warning);
            return;
        }
        // Header
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        // Foldout
        EditorGUI.indentLevel++;
        int contentCount = targetProgress.GetLockCount(out int unlockedCount);
        contentLocksFoldout = EditorGUILayout.Foldout(contentLocksFoldout, "Unlocked " + unlockedCount + "/" + contentCount);
        if (contentLocksFoldout)
        {
            // List
            bool GUIEnabled = GUI.enabled;
            Color GUIColor = GUI.backgroundColor;
            targetProgress.GetObjectiveCount(out int completedObjectiveCount);
            foreach (GameContentLock l in targetProgress.contentLocks)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.ObjectField(l.contentAsset, typeof(ScriptableObject), false);
                if (l.contentAsset == null || l.contentAsset is IUnlockableContent == false)
                {
                    GUI.enabled = true;
                    GUI.backgroundColor = l.locked ? GUIColor : Color.green;
                }
                else if ((l.contentAsset as IUnlockableContent).AutoUnlock)
                {
                    GUI.enabled = false;
                    GUI.backgroundColor = l.locked ? Color.red : Color.green;
                }
                else if (targetProgress.CanUnlockWithObjectives(l.contentAsset, completedObjectiveCount))
                {
                    GUI.enabled = true;
                    GUI.backgroundColor = l.locked ? GUIColor : Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button(l.locked ? "  locked  " : "unlocked")) targetProgress.TrySetLock(l.contentAsset, !l.locked);
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = GUIColor;
            }
            GUI.enabled = GUIEnabled;
        }
        EditorGUI.indentLevel--;
    }
}
