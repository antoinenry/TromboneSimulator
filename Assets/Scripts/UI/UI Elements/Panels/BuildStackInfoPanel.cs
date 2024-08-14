using UnityEngine;

[ExecuteAlways]
public class BuildStackInfoPanel : InfoPanel
{
    public TromboneBuildStack stack;

    public override void UpdateText()
    {
        string text = "";
        // Get description
        TromboneBuildModifier[] mods = stack?.Modifiers;
        int modCount = mods != null ? mods.Length : 0;
        if (modCount == 0)
        {
            text = "Aucune modifs";
        }
        else
        {
            text = "Actif:\n\n";
            for (int i = 0; i < modCount; i++)
                text += "- " + mods[i]?.modName + "\n";
            float scoreMultliplier = stack ? stack.GetScoreMultiplier() : 1f;
            text += "\nScore x" + scoreMultliplier.ToString("0.00");
        }
        // Set field
        if (textField != null) textField.text = text;
    }
}