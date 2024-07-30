using System;
using System.Collections.Generic;

public static class GameContentPicker
{
    public static void PickModifiers(ref TromboneBuildModifier[] modifiers, GameProgress progress = null)
    {
        if (progress == null) progress = GameProgress.Current;
        int requestedCount = modifiers != null ? modifiers.Length : 0;
        if (requestedCount == 0 || progress == null) return;
        List<GameContentLock> available = new List<GameContentLock>(
            Array.FindAll(progress.contentLocks, c =>
            c.locked == true && progress.CanUnlock(c) == true
            && c.contentAsset != null && c.contentAsset is TromboneBuildModifier)
            );
        int availableCount = available != null ? available.Count : 0;
        int m;
        for (m = 0; m < requestedCount; m++)
        {
            if (m >= availableCount)
            {
                modifiers[m] = null;
                continue;
            }
            int randomIndex = UnityEngine.Random.Range(0, availableCount);
            modifiers[m] = available[randomIndex].contentAsset as TromboneBuildModifier;
            available.RemoveAt(randomIndex);
            availableCount--;
        }
    }
}