//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UIElements;

//[CustomEditor(typeof(InstrumentDictionary))]
//public class InstrumentDictionaryInspector : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        InstrumentDictionary dico = target as InstrumentDictionary;
//        if (dico == InstrumentDictionary.Current)
//            dico.isCurrent = true;
//        else
//            dico.isCurrent = false;
//        base.OnInspectorGUI();
//        //if (dico.isCurrent) InstrumentDictionary.Current = dico;
//    }
//}