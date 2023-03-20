using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(TromboneControlWiring))]
public class TromboneControlWiringInspector : Editor
{
    static public float RightColumnWidth => EditorGUIUtility.labelWidth * .85f;
    static public bool showMatrixInspector;

    private TromboneControlWiring targetWiring;
    private string[] inputValueNames;

    private void OnEnable()
    {
        targetWiring = target as TromboneControlWiring;
        inputValueNames = targetWiring.InputValueNames;
    }

    public override void OnInspectorGUI()
    {
        //// Default script line
        //bool enableGUI = GUI.enabled;
        //GUI.enabled = false;
        //EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((TromboneControlWiring)target), typeof(TromboneControlWiring), false);
        //GUI.enabled = enableGUI;

        // Default inspector
        base.OnInspectorGUI();

        // Matrix inspector
        showMatrixInspector = EditorGUILayout.Foldout(showMatrixInspector, "Wire Matrix");
        if (showMatrixInspector)
        {
            // Top line: outputs
            ITromboneOutput[] outputComponents = targetWiring.OutputComponents;
            ValuesLabelLineGUI();
            // Wiring GUI
            if (inputValueNames != null && inputValueNames.Length > 0)
            {
                ITromboneInput[] inputComponents = targetWiring.InputComponents;
                foreach (ITromboneInput input in inputComponents)
                {
                    WiringLineGUI(outputComponents, input);
                }
            }
            else EditorGUILayout.HelpBox("No transmission value types", MessageType.Error);
        }
    }

    public void ValuesLabelLineGUI()
    {
        GUIStyle labelStyle = new(EditorStyles.centeredGreyMiniLabel);
        labelStyle.alignment = TextAnchor.LowerCenter;
        labelStyle.wordWrap = true;
        if (inputValueNames != null)
        {
            int inputValueCount = inputValueNames.Length;
            if (inputValueCount > 0)
            {
                Rect position = EditorGUILayout.GetControlRect();
                Rect labelRect = position;
                labelRect.position += RightColumnWidth * Vector2.right;
                labelRect.width = (position.width - RightColumnWidth) / inputValueCount;
                labelRect.height *= 2f;
                for (int i = 0; i < inputValueCount; i++)
                {
                    EditorGUI.LabelField(labelRect, inputValueNames[i], labelStyle);
                    labelRect.position += labelRect.width * Vector2.right;
                }
                EditorGUILayout.LabelField("");
            }
            else EditorGUILayout.LabelField("No values to transmit", EditorStyles.centeredGreyMiniLabel);
        }
        else EditorGUILayout.LabelField("NULL", EditorStyles.centeredGreyMiniLabel);
    }

    public void WiringLineGUI(ITromboneOutput[] fromComponents, ITromboneInput toComponent)
    {
        Rect position = EditorGUILayout.GetControlRect();
        Rect labelRect = position;
        labelRect.width = RightColumnWidth;
        GUIStyle labelStyle = new(EditorStyles.label);
        labelStyle.alignment = TextAnchor.MiddleRight;
        EditorGUI.LabelField(labelRect, toComponent.ToString(), labelStyle);
        if (fromComponents != null && inputValueNames != null)
        {
            int inputValueCount = inputValueNames.Length;
            if (inputValueCount > 0)
            {
                Rect enumRect = position;
                enumRect.position += (RightColumnWidth + 1f) * Vector2.right;
                enumRect.width = (position.width - RightColumnWidth) / inputValueCount - 2f;
                for (int v = 0; v < inputValueCount; v++)
                {
                    WiringBoxGUI(enumRect, fromComponents, toComponent, inputValueNames[v]);
                    enumRect.position += enumRect.width * Vector2.right;
                }
            }
            else EditorGUI.LabelField(position, "No values to transmit", EditorStyles.centeredGreyMiniLabel);
        }
        else EditorGUI.LabelField(position, "NULL", EditorStyles.centeredGreyMiniLabel);
    }

    public void WiringBoxGUI(Rect position, ITromboneOutput[] fromComponents, ITromboneInput toComponent, string valueName)
    {
        GUIStyle popUpStyle = new(EditorStyles.miniButton);
        popUpStyle.alignment = TextAnchor.MiddleLeft;
        GUIStyle emptyStyle = new(EditorStyles.label);
        emptyStyle.alignment = TextAnchor.MiddleCenter;
        emptyStyle.normal.textColor = Color.gray;
        ITromboneOutput[] outputOptions = Array.FindAll(fromComponents, o => TromboneControlWiring.CanWire(valueName, o, toComponent));
        int optionCount = outputOptions.Length;
        if (optionCount > 0)
        {
            string[] outputNames = new string[optionCount + 1];
            outputNames[0] = "no Input";
            for (int n = 0; n < optionCount; n++) outputNames[n + 1] = outputOptions[n].ToString();
            ITromboneOutput selection = targetWiring.GetWireStart(valueName, toComponent);
            int selectionIndex = Array.IndexOf(outputOptions, selection) + 1;
            EditorGUI.BeginChangeCheck();
            selectionIndex = EditorGUI.Popup(position, selectionIndex, outputNames, selectionIndex == 0 ? emptyStyle : popUpStyle);
            if (EditorGUI.EndChangeCheck())
            {
                if (selectionIndex == 0) targetWiring.SetWire(valueName, null, toComponent);
                else targetWiring.SetWire(valueName, outputOptions[selectionIndex - 1], toComponent);
            }
        }
    }
}
