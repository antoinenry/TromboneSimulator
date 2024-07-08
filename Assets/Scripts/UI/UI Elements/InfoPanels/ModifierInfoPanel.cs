using UnityEngine;

[ExecuteAlways]
public class ModifierInfoPanel : InfoPanel
{
    public TromboneBuildModifier modifierAsset;

    public override void UpdateText()
    {
        string text = "";
        // Get description
        if (modifierAsset != null)
        {
            text += modifierAsset.modName + "\n\n"
                + modifierAsset.description + "\n\n"
                + modifierAsset.StatDescription+ "\n";
        }
        // Set field
        if (textField != null) textField.text = text;
    }
}
