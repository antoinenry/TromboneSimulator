//using UnityEngine;
//using UnityEditor;
//using System;

//[CustomPropertyDrawer(typeof(TromboneControlWiring))]
//public class TromboneControlWiringDrawer : PropertyDrawer
//{
//    static public Type[] transmissionValueTypes;
//    static public int TransmissionValueCount => transmissionValueTypes != null ? transmissionValueTypes.Length : 0;
//    static public float RightColumnWidth => EditorGUIUtility.labelWidth * .85f;

//    private SerializedObject serializedObject;
//    private SerializedProperty[] targetWireProperties;

//    public TromboneControlWiringDrawer()
//    {
//        transmissionValueTypes = TromboneControlWiring.TransmissionValue.GetTypes();
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (TransmissionValueCount + 2);
//    }

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // Target properties
//        serializedObject = property.serializedObject;
//        SerializedProperty wireArrayProperty = property.FindPropertyRelative("wires");
//        int wireCount = wireArrayProperty != null ? wireArrayProperty.arraySize : 0;
//        targetWireProperties = new SerializedProperty[wireCount];
//        for (int w = 0; w < wireCount; w++) targetWireProperties[w] = wireArrayProperty.GetArrayElementAtIndex(w);

//        // Header line
//        Rect lineRect = SingleLineGUI(position);
//        EditorGUI.LabelField(lineRect, "Control Wiring", EditorStyles.boldLabel);
//        lineRect = NextLineGUI(lineRect);

//        // Top line: outputs
//        ITromboneOutput[] outputComponents = GetOutputComponents();
//        ValuesLabelLineGUI(lineRect, transmissionValueTypes);
//        lineRect = NextLineGUI(lineRect);

//        // Wiring GUI
//        if (TransmissionValueCount > 0)
//        {
//            ITromboneInput[] inputComponents = GetInputComponents();
//            foreach (ITromboneInput input in inputComponents)
//            {
//                WiringLineGUI(lineRect, outputComponents, input, transmissionValueTypes);
//                lineRect = NextLineGUI(lineRect);
//            }
//        }
//        else EditorGUILayout.HelpBox("No transmission value types", MessageType.Error);
//    }

//    public void ValuesLabelLineGUI(Rect position, Type[] valueTypes)
//    {
//        GUIStyle labelStyle = new(EditorStyles.centeredGreyMiniLabel);
//        labelStyle.alignment = TextAnchor.LowerCenter;
//        if (valueTypes != null)
//        {
//            int typeCount = valueTypes.Length;
//            if (typeCount > 0)
//            {
//                string[] names = Array.ConvertAll(valueTypes, v => v.Name);
//                Rect labelRect = position;
//                labelRect.position += RightColumnWidth * Vector2.right;
//                labelRect.width = (position.width - RightColumnWidth) / typeCount;
//                for (int i = 0; i < typeCount; i++)
//                {
//                    EditorGUI.LabelField(labelRect, names[i], labelStyle);
//                    labelRect.position += labelRect.width * Vector2.right;
//                }
//            }
//            else EditorGUI.LabelField(position, "No values to transmit", EditorStyles.centeredGreyMiniLabel);
//        }
//        else EditorGUI.LabelField(position, "NULL", EditorStyles.centeredGreyMiniLabel);
//    }

//    public void WiringLineGUI(Rect position, ITromboneOutput[] fromComponents, ITromboneInput toComponent, Type[] valueTypes)
//    {
//        Rect labelRect = position;
//        labelRect.width = RightColumnWidth;
//        GUIStyle labelStyle = new(EditorStyles.label);
//        labelStyle.alignment = TextAnchor.MiddleRight;
//        EditorGUI.LabelField(labelRect, toComponent.ToString(), labelStyle);
//        if (fromComponents != null && valueTypes != null)
//        {
//            int typeCount = valueTypes.Length;
//            if (typeCount > 0)
//            {
//                int outputCount = fromComponents.Length;
//                string[] outputNames = new string[outputCount + 1];
//                outputNames[0] = "OFF";
//                for (int n = 0; n < outputCount; n++) outputNames[n + 1] = fromComponents[n].ToString();
//                Rect enumRect = position;
//                enumRect.position += (RightColumnWidth + 1f) * Vector2.right;
//                enumRect.width = (position.width - RightColumnWidth) / typeCount - 2f;
//                GUIStyle popUpStyle = EditorStyles.toolbarPopup;
//                popUpStyle.alignment = TextAnchor.MiddleLeft;
//                for (int v = 0; v < typeCount; v++)
//                {
//                    ITromboneOutput selection = GetWireStart(transmissionValueTypes[v], toComponent);
//                    int selectionIndex = Array.IndexOf(fromComponents, selection) + 1;
//                    EditorGUI.BeginChangeCheck();
//                    selectionIndex = EditorGUI.Popup(enumRect, selectionIndex, outputNames, popUpStyle);
//                    enumRect.position += enumRect.width * Vector2.right;
//                    if (EditorGUI.EndChangeCheck())
//                    {
//                        if (selectionIndex == 0) SetWire(valueTypes[v], null, toComponent);
//                        else SetWire(valueTypes[v], fromComponents[selectionIndex - 1], toComponent);
//                    }
//                }
//            }
//            else EditorGUI.LabelField(position, "No values to transmit", EditorStyles.centeredGreyMiniLabel);
//        }
//        else EditorGUI.LabelField(position, "NULL", EditorStyles.centeredGreyMiniLabel);
//    }

//    private Rect SingleLineGUI(Rect position)
//    {
//        Rect singleLineRect = position;
//        singleLineRect.height = EditorGUIUtility.singleLineHeight;
//        return singleLineRect;
//    }

//    private Rect NextLineGUI(Rect position)
//    {
//        Rect nextLine = position;
//        nextLine.position += (position.height + EditorGUIUtility.standardVerticalSpacing) * Vector2.up;
//        return nextLine;
//    }

//    private ITromboneInput[] GetInputComponents()
//    {
//        if (serializedObject.targetObject is UnityEngine.Object) return (serializedObject.targetObject as MonoBehaviour).GetComponentsInChildren<ITromboneInput>(true);
//        else return null;
//    }

//    private ITromboneOutput[] GetOutputComponents()
//    {
//        if (serializedObject.targetObject is MonoBehaviour) return (serializedObject.targetObject as MonoBehaviour).GetComponentsInChildren<ITromboneOutput>(true);
//        else return null;
//    }

//    private ITromboneOutput GetWireStart(Type valueType, ITromboneInput wireEnd)
//    {
//        if (targetWireProperties != null)
//        {
//            int wireIndex = Array.FindIndex(targetWireProperties, w => IsWireConnectedTo(w, valueType, wireEnd));
//            if (wireIndex != -1) return GetWireStart(targetWireProperties[wireIndex]);
//        }
//        return null;
//    }

//    private ITromboneOutput GetWireStart(SerializedProperty wireProperty)
//    {
//        if (wireProperty != null)
//        {
//            UnityEngine.Object wireStartReference = wireProperty.FindPropertyRelative("from").objectReferenceValue;
//            if (wireStartReference != null && wireStartReference is ITromboneOutput) return wireStartReference as ITromboneOutput;
//        }
//        return null;
//    }

//    private bool IsWireConnectedTo(SerializedProperty wireProperty, Type valueType, ITromboneInput to)
//    {
//        if (wireProperty != null)
//        {
//            string wireValueTypeName = wireProperty.FindPropertyRelative("valueType").stringValue;
//            if (valueType == null || valueType.Name != wireValueTypeName) return false;
//            UnityEngine.Object wireEndReference = wireProperty.FindPropertyRelative("to").objectReferenceValue;
//            if (wireEndReference != null && wireEndReference is ITromboneInput) return (wireEndReference as ITromboneInput) == to;
//        }
//        return false;
//    }

//    private void SetWire(Type valueType, ITromboneOutput from, ITromboneInput to)
//    {
//        Trombone targetTrombone = serializedObject.targetObject as Trombone;
//        if (targetTrombone != null) targetTrombone.controlWiring.SetWire(valueType, from, to);
//    }

//    //private void LabelLineGUI(Rect position, string lineLabel)
//    //{
//    //    EditorGUI.DrawRect(position, Color.gray * .6f);
//    //    EditorGUI.LabelField(position, lineLabel, EditorStyles.boldLabel);
//    //    Rect textRect = position;
//    //    textRect.position += EditorGUIUtility.labelWidth * Vector2.right;
//    //    textRect.width = (position.width - EditorGUIUtility.labelWidth) / Rows;
//    //    foreach (string wireTo in wireToProperties)
//    //    {
//    //        EditorGUI.LabelField(textRect, wireTo, EditorStyles.boldLabel);
//    //        textRect.position += textRect.width * Vector2.right;
//    //    }
//    //}

//    //private void WireGUI(Rect position, SerializedProperty property, string wireFrom)
//    //{
//    //    Rect singleLineRect = position;
//    //    foreach (string wireValue in wireValueProperties)
//    //    {
//    //        SerializedProperty flagProperty = property.FindPropertyRelative(wireFrom).FindPropertyRelative(wireValue);
//    //        EditorGUI.BeginChangeCheck();
//    //        int flagsIntValue = FlagLineGUI(singleLineRect, wireValue, flagProperty.enumValueFlag);
//    //        if (EditorGUI.EndChangeCheck())
//    //        {
//    //            Undo.RecordObject(property.serializedObject.targetObject, "Wire Trombone");
//    //            flagProperty.enumValueFlag = flagsIntValue;
//    //            EditorUtility.SetDirty(property.serializedObject.targetObject);
//    //        }
//    //        singleLineRect.position += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * Vector2.up;
//    //    }
//    //}

//    //private int FlagLineGUI(Rect position, string lineLabel, int flagsInt)
//    //{
//    //    EditorGUI.LabelField(position, lineLabel);
//    //    Rect toggleRect = position;
//    //    toggleRect.width = (position.width - EditorGUIUtility.labelWidth) / Rows;
//    //    toggleRect.position += EditorGUIUtility.labelWidth * Vector2.right;

//    //    TromboneControlWiring.WireTo flags = (TromboneControlWiring.WireTo)flagsInt;

//    //    bool toTrombone = flags.HasFlag(TromboneControlWiring.WireTo.ToTrombone);
//    //    toTrombone = EditorGUI.Toggle(toggleRect, toTrombone);
//    //    toggleRect.position += toggleRect.width * Vector2.right;

//    //    bool toDisplay = flags.HasFlag(TromboneControlWiring.WireTo.ToDisplay);
//    //    toDisplay = EditorGUI.Toggle(toggleRect, toDisplay);
//    //    toggleRect.position += toggleRect.width * Vector2.right;

//    //    bool toAudio = flags.HasFlag(TromboneControlWiring.WireTo.ToAudio);
//    //    toAudio = EditorGUI.Toggle(toggleRect, toAudio);
//    //    toggleRect.position += toggleRect.width * Vector2.right;

//    //    TromboneControlWiring.WireTo resultFlags = 0;
//    //    if (toTrombone) resultFlags |= TromboneControlWiring.WireTo.ToTrombone;
//    //    if (toDisplay) resultFlags |= TromboneControlWiring.WireTo.ToDisplay;
//    //    if (toAudio) resultFlags |= TromboneControlWiring.WireTo.ToAudio;
//    //    return (int)resultFlags;
//    //}
//}
