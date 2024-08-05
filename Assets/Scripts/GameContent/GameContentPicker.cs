using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameContentPicker
{
    public static TromboneBuildModifier[] PickUnlockableModifiers(int requestedCount, GameProgress progress = null, bool prioritizeHighTier = true)
    {
        if (progress == null) progress = GameProgress.Current;
        if (requestedCount <= 0 || progress == null) return new TromboneBuildModifier[0];
        // Get unlockable content
        List<TromboneBuildModifier> available = new List<GameContentLock>(
            Array.FindAll(progress.contentLocks, c =>
            c.locked == true && progress.CanUnlock(c) == true && c.contentAsset != null && c.contentAsset is TromboneBuildModifier))
            .ConvertAll(c => c.contentAsset as TromboneBuildModifier);
        // No available content
        if (available == null || available.Count == 0) return new TromboneBuildModifier[0];
        // Prioritize by tier
        if (prioritizeHighTier)
        {
            List<TromboneBuildModifier> outputModifier = new List<TromboneBuildModifier>(requestedCount);
            int outputCount = 0;
            progress.GetObjectiveCount(out int unlockTier);
            while (outputCount < requestedCount && unlockTier >= 0)
            {
                TromboneBuildModifier[] tierModifiers = PickModifiers(requestedCount - outputCount, available.FindAll(m => m != null && m.unlockTier == unlockTier).ToArray());
                outputModifier.AddRange(tierModifiers);
                outputCount += tierModifiers.Length;
                unlockTier--;
            }
            return outputModifier.ToArray();
        }
        // Or pick from all available modifiers
        else
        {
            return PickModifiers(requestedCount, available.ToArray());
        }
    }

    public static TromboneBuildModifier[] PickModifiers(int requestedCount, TromboneBuildModifier[] modifierPool)
    {
        List<TromboneBuildModifier> available = new List<TromboneBuildModifier>(modifierPool);
        available.RemoveAll(m => m == null);
        int availableCount = available != null ? available.Count : 0;
        int outputCount = Mathf.Min(requestedCount, availableCount);
        TromboneBuildModifier[] outputModifiers = new TromboneBuildModifier[outputCount];
        for (int m = 0; m < outputCount; m++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCount);
            outputModifiers[m] = available[randomIndex];
            available.RemoveAt(randomIndex);
            availableCount--;
        }
        return outputModifiers;
    }
}