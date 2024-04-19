//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(Level))]
//public class LevelInspector : Editor
//{
//    private Level level;

//    private void OnEnable()
//    {
//        level = target as Level;    
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.Space();
//        if (GUILayout.Button("Refresh")) (target as Level).FindResourcesByName();
//        EditorGUILayout.Space();
//        EditorGUILayout.EndHorizontal();
//    }
//}