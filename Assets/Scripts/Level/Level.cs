using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Trombone Hero/Game Data/Level")]
public class Level : ScriptableObject, IUnlockableContent
{
    //public string levelName;
    public SheetMusic music;
    public ObjectiveList objectives;
    public int unlockTier;
    //public NotePlacement[] notePlacement;

    public int ObjectiveCount => objectives != null ? objectives.Count : 0;
    public bool AutoUnlock => true;
    public int UnlockTier => unlockTier;

    //public string Name => music != null ? music.name : null;

    //public void FindResourcesByName()
    //{
    //    if (levelName != null)
    //    {
    //        List<NotePlacement> findNotePlacements = new List<NotePlacement>();
    //        SheetMusic[] allSheetMusics = Resources.FindObjectsOfTypeAll<SheetMusic>();
    //        music = Array.Find(allSheetMusics, m => m.name.Contains(levelName));
    //        NotePlacementAsset[] allPlacementAssets = Resources.FindObjectsOfTypeAll<NotePlacementAsset>();
    //        foreach(NotePlacementAsset asset in allPlacementAssets)
    //        {
    //            if (asset.notePlacements != null && asset.name.Contains(levelName)) 
    //                findNotePlacements.AddRange(asset.notePlacements); 
    //        }
    //        notePlacement = findNotePlacements.ToArray();
    //    }
    //}
}