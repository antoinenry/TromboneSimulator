using UnityEditor;
using System;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectiveInfo))]
public class ObjectiveInfoDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ObjectiveInfo target = GetDescriptor(property);
        int parameterCount = target.parameters != null ? target.parameters.Length : 0;
        return base.GetPropertyHeight(property, label) + parameterCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect drawingRect = position;
        drawingRect.height = EditorGUIUtility.singleLineHeight;
        ObjectiveInfo target = GetDescriptor(property);
        // Edit objective type
        string[] typeNames = ObjectiveInstance.GetAllTypeNames();
        int selectedIndex = Array.IndexOf(typeNames, target.type);
        EditorGUI.BeginChangeCheck();
        selectedIndex = EditorGUI.Popup(drawingRect, "", selectedIndex, typeNames);
        if (EditorGUI.EndChangeCheck())
        {
            target = ObjectiveInfo.New(typeNames[selectedIndex]);
            property.FindPropertyRelative("type").stringValue = target.type;
            property.FindPropertyRelative("parameters").arraySize = target.parameters.Length;
        }
        if (target.type == null) return;
        // Edit objective parameters
        string[] parameterNames = target.GetParameterNames();
        Type[] parameterTypes = target.GetParameterTypes();
        EditorGUI.indentLevel++;
        for (int i = 0, iend = parameterNames.Length; i < iend; i++)
        {
            EditorGUI.BeginChangeCheck();
            drawingRect.position = position.position + (i + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * Vector2.up;
            string parameterValue = ParameterFieldGUI(drawingRect, parameterNames[i], target.parameters[i], parameterTypes[i]);
            if (EditorGUI.EndChangeCheck()) property.FindPropertyRelative("parameters").GetArrayElementAtIndex(i).stringValue = parameterValue;
        }
        EditorGUI.indentLevel--;
    }

    private ObjectiveInfo GetDescriptor(SerializedProperty property)
    {
        string type = property.FindPropertyRelative("type").stringValue;
        ObjectiveInfo o = ObjectiveInfo.New(type);
        SerializedProperty serializedParameters = property.FindPropertyRelative("parameters");
        int parameterCount = serializedParameters != null ? serializedParameters.arraySize : 0;
        string[] parameterValues = new string[parameterCount];
        for (int i = 0; i < parameterCount; i++) parameterValues[i] = serializedParameters.GetArrayElementAtIndex(i).stringValue;
        o.parameters = parameterValues;        
        return o.IsValid() ? o : ObjectiveInfo.New(type);
    }

    private string ParameterFieldGUI(Rect position, string label, string value, Type type)
    {
        if (type == typeof(int))
        {
            int.TryParse(value, out int parsed);
            return EditorGUI.IntField(position, label, parsed).ToString();
        }
        if (type == typeof(float))
        {
            float.TryParse(value, out float parsed);
            return EditorGUI.FloatField(position, label, parsed).ToString();
        }
        if (type == typeof(bool))
        {
            bool.TryParse(value, out bool parsed);
            return EditorGUI.Toggle(position, label,parsed).ToString();
        }
        return EditorGUI.TextField(position, label, value);
    }
}
