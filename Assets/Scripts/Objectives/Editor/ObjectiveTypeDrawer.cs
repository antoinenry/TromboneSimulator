using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(ObjectiveTypeAttribute))]
public class ObjectiveTypeDrawer : PropertyDrawer
{
    private int selectedIndex;   

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            base.OnGUI(position, property, label);
            return;
        }
        string selectedTypeName = property.stringValue;
        string[] typeNames = ObjectiveInstance.GetAllTypeNames();
        selectedIndex = EditorGUI.Popup(position, Array.IndexOf(typeNames, selectedTypeName), typeNames);
        property.stringValue = typeNames[selectedIndex];
    }
}
