using UnityEngine;

// A small addition to ScriptableObject
// Helps creating objects that allow only one instance to be active, kinda like a singleton.
// When set to true, the "isCurrent" field will automatically be set to false in other instances.
// The static reference to the Current instance must be implemented in the children classes with the following two lines.
// Implementation example ---------------------------------------------
//public class ExampleObject : SingleScriptableObject
//{
//    protected override ExampleObject CurrentObject { get => Current; set => Current = value as ExampleObject; }
//    static public ExampleObject Current;
//}
// --------------------------------------------------------------------
//
// This might be made more efficient with Reflection but I'll keep it simple like this for now.

public abstract class SingleScriptableObject : ScriptableObject
{
    [SerializeField] protected bool isCurrent;
    protected abstract SingleScriptableObject CurrentObject { get; set; }

    protected virtual void OnValidate() => SetCurrent();
    protected virtual void OnEnable() => SetCurrent();
    protected virtual void OnDisable() => SetCurrent();

    private void SetCurrent()
    {
        if (isCurrent)
        {
            if (CurrentObject != null && CurrentObject != this) CurrentObject.isCurrent = false;
            CurrentObject = this;
        }
        else
        {
            if (CurrentObject == this) CurrentObject = null;
        }
    }
}
