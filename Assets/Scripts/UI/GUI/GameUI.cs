using UnityEngine;

[ExecuteAlways]
public abstract class GameUI : MonoBehaviour
{
    public abstract Component[] UIComponents { get; }

    public virtual bool GUIActive
    {
        get
        {
            // Check if each GUI component is active
            if (UIComponents != null) foreach (Component c in UIComponents)
                    if (c && c.gameObject.activeInHierarchy == false) return false;
            return true;
        }

        set
        {
            // Activate/Deactivate every GUI component
            if (UIComponents != null) foreach (Component c in UIComponents)
                    if (c) c.gameObject.SetActive(value);
        }
    }
}