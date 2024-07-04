using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Trombone Hero/Game Data/Level")]
public class Level : ScriptableObject, IUnlockableContent
{
    //public string levelName;
    public SheetMusic music;
    //public ObjectiveList objectiveList;
    public int unlockTier;
    //public NotePlacement[] notePlacement;
    public ObjectiveInfo[] objectives;

    public bool AutoUnlock => true;
    public int UnlockTier => unlockTier;
    public float MusicDuration => music != null ? music.GetDuration() : 0;

    public bool TryGetCurrentProgress(out int completedObjectives, out int totalObjectives)
    {
        GameProgress currentProgress = GameProgress.Current;
        if (currentProgress == null)
        {
            completedObjectives = 0;
            totalObjectives = 0;
            return false;
        }
        return currentProgress.TryGetLevelProgress(this, out completedObjectives, out totalObjectives);
    }
}