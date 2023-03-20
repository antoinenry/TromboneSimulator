//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(NoteGrid))]
//public class NoteGridInspector : Editor
//{    
//    public override void OnInspectorGUI()
//    {
//        NoteGrid grid = target as NoteGrid;
//        EditorGUI.BeginChangeCheck();
//        base.OnInspectorGUI();
//        if (EditorGUI.EndChangeCheck() == true|| grid.transform.hasChanged)
//        {
//            grid.UpdateDimensions();
//            grid.transform.hasChanged = false;
//        }
//    }
//}