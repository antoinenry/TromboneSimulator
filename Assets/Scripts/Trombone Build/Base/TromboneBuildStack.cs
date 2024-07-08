using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TromboneBuildStack : MonoBehaviour
{
    public TromboneBuild baseBuild;
    public bool enableModifiers = true;

    [SerializeField] private List<TromboneBuildModifier> mods;
    [SerializeField] private TromboneBuild modifiedBuild;

    private void OnValidate()
    {
        ApplyStack();
    }

    public TromboneBuildModifier[] Modifiers =>  mods != null ? mods.ToArray() : new TromboneBuildModifier[0];

    public bool TryAddModifier(TromboneBuildModifier mod)
    {
        if (mods == null || mod == null) return false;
        mods.RemoveAll(m => m == null || m.CanStackWith(mod) == false);
        mods.Add(mod);
        return true;
    }

    public bool TryRemoveModifier(TromboneBuildModifier mod)
    {
        if (mods == null || mod == null) return false;
        return mods.Remove(mod);
    }

    public void ApplyStack()
    {
        if (modifiedBuild == null) modifiedBuild = ScriptableObject.CreateInstance<TromboneBuild>();
        if (baseBuild != null) TromboneBuild.Copy(baseBuild, modifiedBuild);
        else modifiedBuild.GetBuildFromScene();
        if (enableModifiers)
            foreach (TromboneBuildModifier modifier in mods)
                if (modifier != null) modifier.ApplyTo(modifiedBuild);
        modifiedBuild.SetBuildToScene();
    }

    public void CleanUp()
    {
        if (IsStackClean(out int[] nullMods, out int[][] cantStack)) return;
        // Replace unstackables with a null, keeping only the last one in the list
        foreach (int[] csi in cantStack)
            for (int i = 0; i < csi.Length - 1; i++)
                mods[csi[i]] = null;
        // Remove nulls
        mods.RemoveAll(m => m == null);
    }

    public bool IsStackClean(out int[] nullMods, out int[][] cantStack)
    {
        nullMods = new int[0];
        cantStack = new int[0][];
        int modCount = mods != null ? mods.Count : 0;
        if (modCount == 0) return true;
        // Find indices of null mods and group unstackables mods by types
        List<int> nullIndices = new List<int>();
        List<List<int>> cantStackIndices = new List<List<int>>();
        for (int i = 0; i < modCount; i++)
        {
            if (mods[i] == null)
            {
                nullIndices.Add(i);
                continue;
            }
            //if (mods[i].CanStack)
            //{
            //    continue;
            //}
            //Type type = mods[i].GetType();
            //int typeIndex = cantStackIndices.FindIndex(csi => mods[csi[0]].GetType() == type);
            //if (typeIndex == -1) cantStackIndices.Add(new List<int>() { i });
            //else cantStackIndices[typeIndex].Add(i);
            int groupIndex = cantStackIndices.FindIndex(csi => mods[csi[0]].CanStackWith(mods[i]) == false);
            if (groupIndex == -1) cantStackIndices.Add(new List<int>() { i });
            else cantStackIndices[groupIndex].Add(i);
        }
        // If unstackable mods are just one of each type, it's ok, remove them from the list
        cantStackIndices.RemoveAll(csi => csi.Count == 1);
        // Stack is clean if there's no null mods and no conflicts between unstackable mods
        if (nullIndices.Count == 0 && cantStackIndices.Count == 0) return true;
        // Else return problematic indices
        nullMods = nullIndices.ToArray();
        cantStack = cantStackIndices.ConvertAll(csi => csi.ToArray()).ToArray();
        return false;
    }

    public float GetScoreMultiplier()
    {
        if (mods == null) return 1f;
        float multiplier = 1f;
        foreach (TromboneBuildModifier m in mods)
            if (m != null) multiplier *= m.ScoreMultiplier;
        return multiplier;
    }
}