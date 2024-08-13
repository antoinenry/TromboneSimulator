using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(ToneAttribute))]
public class ToneAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Set rects
        Rect valueFieldPosition = position;
        valueFieldPosition.width *= .5f;
        Rect nameFieldPosition = position;
        nameFieldPosition.x += valueFieldPosition.width;
        nameFieldPosition.width -= valueFieldPosition.width;
        // Regular field
        string selectedNoteName = "...";
        if (property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = EditorGUI.FloatField(valueFieldPosition, label, property.floatValue);
            selectedNoteName = ToneAttribute.GetNoteName(property.floatValue, (attribute as ToneAttribute).hideDrumHit);
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = EditorGUI.IntField(valueFieldPosition, label, property.intValue);
            selectedNoteName = ToneAttribute.GetNoteName(property.intValue, (attribute as ToneAttribute).hideDrumHit);
        }
        else if (property.propertyType == SerializedPropertyType.String)
        {
            float floatTone = ToneAttribute.GetNoteTone(property.stringValue);
            floatTone = EditorGUI.FloatField(valueFieldPosition, label, floatTone);
            property.stringValue = ToneAttribute.GetNoteName(floatTone, (attribute as ToneAttribute).hideDrumHit);
            selectedNoteName = property.stringValue;
        }
        // Note name field (letter and number)
        EditorGUI.BeginChangeCheck();
        selectedNoteName = EditorGUI.TextField(nameFieldPosition, selectedNoteName);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(property.serializedObject.targetObject, "Edit tone");
            if (property.propertyType == SerializedPropertyType.Float)
                property.floatValue = ToneAttribute.GetNoteTone(selectedNoteName);
            else if (property.propertyType == SerializedPropertyType.Integer)
                property.intValue = (int)ToneAttribute.GetNoteTone(selectedNoteName);
            else if (property.propertyType == SerializedPropertyType.String)
                property.stringValue = selectedNoteName;
        }
    }

    static public float GUIToneField(string label, float value, bool hideDrumHit = false)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        value = EditorGUILayout.FloatField(value);
        EditorGUI.BeginChangeCheck();
        string selectedNoteName = EditorGUILayout.TextField(ToneAttribute.GetNoteName(value, hideDrumHit));
        if (EditorGUI.EndChangeCheck()) value = ToneAttribute.GetNoteTone(selectedNoteName);
        EditorGUILayout.EndHorizontal();
        return value;
    }
}