//using UnityEngine;
//using UnityEditor;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//[CustomEditor(typeof(ObjectiveJudgeInspector))]
//public class ObjectiveJudgeInspector : Editor
//{
//    private ObjectiveJudge targetJudge;
//    private Type[] objectiveTypes;
//    private int selectedTypeIndex;

//    private void OnEnable()
//    {
//        targetJudge = target as ObjectiveJudge;
//        objectiveTypes = GetAllObjectiveTypes();
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();
//        selectedTypeIndex = EditorGUILayout.Popup("Add type", )
//    }

//    private Type[] GetAllObjectiveTypes()
//        => Array.FindAll(Assembly.GetExecutingAssembly().GetTypes(), t => !t.IsAbstract && t.IsAssignableFrom(typeof(Objective));
//}
