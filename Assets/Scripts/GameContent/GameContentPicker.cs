using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameContentPicker : MonoBehaviour
{
    [Flags]
    public enum PickMode { PrioritizeHighTier = 1, PrioritizeDifferentTiers = 2}

    public TromboneBuildModifier[] picks;
    public PickMode pickMode;

    public ObjectMethodCaller caller = new("Pick");

    public void Pick()
    {
        int requestedCount = picks != null ? picks.Length : 0;
        if (requestedCount > 0) picks = PickUnlockableModifiers(requestedCount, GameProgress.Current, pickMode.HasFlag(PickMode.PrioritizeHighTier), pickMode.HasFlag(PickMode.PrioritizeDifferentTiers));
    }

    public static TromboneBuildModifier[] PickUnlockableModifiers(int requestedCount, GameProgress progress = null, bool prioritizeHighTier = true, bool prioritizeDifferentTiers = true)
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
        // Priorities
        progress.GetObjectiveCount(out int currentTier);
        if (prioritizeHighTier && prioritizeDifferentTiers)
        {
            List<TromboneBuildModifier> outputMods = new List<TromboneBuildModifier>(requestedCount);
            // Separate high tiers mods and others
            List<TromboneBuildModifier> highTierMods = new(PickModifiers(requestedCount, PrioritizeHighTier(requestedCount, currentTier, available.ToArray())));
            List<TromboneBuildModifier> differentTierMods = new(PickModifiers(requestedCount, PrioritizeDifferentTiers(requestedCount, available.ToArray())));
            foreach (TromboneBuildModifier mod in highTierMods) differentTierMods.Remove(mod);
            // Try picking at least one high tier mod
            if (highTierMods.Count > 0)
            {
                TromboneBuildModifier mod = PickModifier(highTierMods.ToArray());
                outputMods.Add(mod);
                highTierMods.Remove(mod);
            }
            if (outputMods.Count >= requestedCount) return outputMods.ToArray();
            // Try completing with other mods
            outputMods.AddRange(PickModifiers(requestedCount - outputMods.Count, differentTierMods.ToArray()));
            if (outputMods.Count >= requestedCount) return outputMods.ToArray();
            // If still not enough picks, try completing with high tier again
            outputMods.AddRange(PickModifiers(requestedCount - outputMods.Count, highTierMods.ToArray()));
            return outputMods.ToArray();
        }
        if (prioritizeHighTier)
            return PickModifiers(requestedCount, PrioritizeHighTier(requestedCount, currentTier, available.ToArray()));
        if (prioritizeDifferentTiers)
            return PickModifiers(requestedCount, PrioritizeDifferentTiers(requestedCount, available.ToArray()));
        // No priority settings
        return PickModifiers(requestedCount, available.ToArray());
    }

    public static TromboneBuildModifier[] PickModifiers(int requestedCount, TromboneBuildModifier[] modifierPool)
    {
        List<TromboneBuildModifier> available = new List<TromboneBuildModifier>(modifierPool);
        TromboneBuildModifier[] outputModifiers = InitializeOutput(requestedCount, ref available, out int availableCount, out int outputCount);
        for (int m = 0; m < outputCount; m++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCount);
            outputModifiers[m] = available[randomIndex];
            available.RemoveAt(randomIndex);
            availableCount--;
        }
        return outputModifiers;
    }

    public static TromboneBuildModifier PickModifier(TromboneBuildModifier[] modifierPool)
    {
        TromboneBuildModifier[] outputArray = PickModifiers(1, modifierPool);
        return outputArray != null && outputArray.Length > 0 ? outputArray[0] : null;
    }

    private static TromboneBuildModifier[] InitializeOutput(int requestedCount, ref List<TromboneBuildModifier> input, out int inputCount, out int outputCount)
    {
        inputCount = input != null ? input.Count : 0;
        if (inputCount != 0) input.RemoveAll(m => m == null);
        outputCount = Mathf.Min(requestedCount, inputCount);
        return new TromboneBuildModifier[outputCount];
    }

    private static TromboneBuildModifier[] PrioritizeDifferentTiers(int requestedCount, TromboneBuildModifier[] modifierPool)
    {
        List<TromboneBuildModifier> availableMods = new List<TromboneBuildModifier>(modifierPool);
        TromboneBuildModifier[] outputModifiers = InitializeOutput(requestedCount, ref availableMods, out int availableCount, out int outputCount);
        // Get all possible tier values
        List<int> availableTiers = new List<int>(availableCount);
        foreach (TromboneBuildModifier modifier in availableMods)
            if (availableTiers.Contains(modifier.UnlockTier) == false) availableTiers.Add(modifier.UnlockTier);
        int availableTierCount = availableTiers.Count;
        // Try to get one modifier per tier (with random pick)
        int outputIndex = 0;
        for (; outputIndex < outputCount; outputIndex++)
        {
            if (availableTierCount <= 0) break;
            int randomTierIndex = UnityEngine.Random.Range(0, availableTierCount);
            int randomTier = availableTiers[randomTierIndex];
            TromboneBuildModifier mod = PickModifier(availableMods.FindAll(m => m != null && m.unlockTier == randomTier).ToArray());
            availableMods.Remove(mod);
            outputModifiers[outputIndex] = mod;
            availableTiers.Remove(randomTier);
            availableTierCount--;
        }
        // If enough modifiers were found, stop here
        if (outputIndex == outputCount) return outputModifiers;
        // Or complete with random mods
        TromboneBuildModifier[] completeMods = PickModifiers(outputCount - outputIndex, availableMods.ToArray());
        for (int i = 0; outputIndex < outputCount; outputIndex++)
            outputModifiers[outputIndex] = completeMods[i++];
        return outputModifiers.ToArray();
    }

    private static TromboneBuildModifier[] PrioritizeHighTier(int requestedCount, int maxTier, TromboneBuildModifier[] modifierPool)
    {
        List<TromboneBuildModifier> outputModifiers = new List<TromboneBuildModifier>(requestedCount);
        int unlockTier = maxTier, outputCount = 0;
        while (outputCount < requestedCount && unlockTier >= 0)
        {
            TromboneBuildModifier[] tierModifiers = PickModifiers(requestedCount - outputCount, Array.FindAll(modifierPool, m => m != null && m.unlockTier == unlockTier).ToArray());
            outputModifiers.AddRange(tierModifiers);
            outputCount += tierModifiers.Length;
            unlockTier--;
        }
        return outputModifiers.ToArray();
    }
}