using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CurrentToggleAttribute))]
public class CurrentToggleDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Boolean)
        {
            Object target = property.serializedObject.targetObject;
            EditorUtility.SetDirty(target);
            bool isCurrent = CurrentAssetsManager.IsCurrent(target);
            EditorGUI.BeginChangeCheck();
            isCurrent = EditorGUI.Toggle(position, label, isCurrent);
            if (EditorGUI.EndChangeCheck())
            {
                if (isCurrent) CurrentAssetsManager.SetCurrent(target);
                else CurrentAssetsManager.RemoveCurrent(target);
            }
        }
        else
            EditorGUI.LabelField(position, label.text, "Error: must be boolean.");
    }
}