using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomPropertyDrawer(typeof(ObjectMethodCaller))]
public class ObjectMethodCallerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty methodsProperty = property.FindPropertyRelative("methods");
        int? methodCount = methodsProperty?.arraySize;
        if (methodCount.HasValue && methodCount > 0)
        {
            Rect buttonPosition = position;
            buttonPosition.width = position.width / methodCount.Value;
            for (int m = 0; m < methodCount; m++)
            {
                string methodName = methodsProperty.GetArrayElementAtIndex(m).stringValue;
                if (GUI.Button(buttonPosition, methodName)) CallMethod(property, methodName);
                buttonPosition.x += buttonPosition.width;
            }
        }
        else
        {
            EditorGUI.HelpBox(position, "Add methods in Debug view.", MessageType.Info);
        }
    }

    private void CallMethod(SerializedProperty property, string methodName)
    {
        MethodInfo callMethod = typeof(ObjectMethodCaller).GetMethod("CallMethod", BindingFlags.Static | BindingFlags.Public);
        object[] parameters = new object[] { property.serializedObject.targetObject, methodName };
        callMethod.Invoke(property.serializedObject.targetObject, parameters);
    }
}
