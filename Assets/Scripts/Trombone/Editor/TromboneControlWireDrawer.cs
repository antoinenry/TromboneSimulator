using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TromboneControlWiring.Wire))]
public class TromboneControlWireDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float spaceWidth = 20f;
        Rect fieldRect = position;
        fieldRect.width = position.width / 3f;

        SerializedProperty valueTypeProperty = property.FindPropertyRelative("valueType");
        EditorGUI.LabelField(fieldRect, valueTypeProperty.stringValue);

        fieldRect.width -= spaceWidth / 2f;
        fieldRect.position += Vector2.right * fieldRect.width;

        GUI.enabled = false;
        SerializedProperty fromProperty = property.FindPropertyRelative("from");
        EditorGUI.ObjectField(fieldRect, fromProperty.objectReferenceValue, typeof(ITromboneOutput), true);

        Rect spaceRect = fieldRect;
        spaceRect.position += Vector2.right * fieldRect.width;
        spaceRect.width = spaceWidth;
        EditorGUI.LabelField(spaceRect, " to ");

        fieldRect.position = spaceRect.position + spaceWidth * Vector2.right;

        SerializedProperty toProperty = property.FindPropertyRelative("to");
        EditorGUI.ObjectField(fieldRect, toProperty.objectReferenceValue, typeof(ITromboneInput), true);
        GUI.enabled = true;
    }
}