using UnityEngine;
using System;
using System.Reflection;

[CreateAssetMenu(fileName = "NewTrombone", menuName = "Trombone Hero/Instruments/Trombone Build")]
public class TromboneBuild : ScriptableObject
{
    public TromboneCoreCustomizer tromboneCore = new();
    public TromboneDisplayCustomizer tromboneDisplay = new();
    public TromboneAudioCustomizer tromboneAudio = new();
    public TromboneAutoCustomizer tromboneAuto = new();
    public MusicPlayerCustomizer musicPlayer = new();
    public PerformanceJudgeCustomizer performanceJudge = new();

    public static void Copy(TromboneBuild source, TromboneBuild destination)
    {
        if (destination == null || source == null) return;
        FieldInfo[] customizers = typeof(TromboneBuild).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        foreach (FieldInfo customizer in customizers)
        {
            object sourceCustomizer = customizer.GetValue(source);
            object destinationCustomizer = customizer.GetValue(destination);
            Type cutomizerType = sourceCustomizer.GetType();
            ValueCopier.CopyValuesByName(cutomizerType, sourceCustomizer, destinationCustomizer);
        }
    }

    public void SetBuildToScene()
    {
        FieldInfo[] customizers = typeof(TromboneBuild).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        foreach (FieldInfo customizer in customizers)
        {
            object sourceCustomizer = customizer.GetValue(this);
            Type componentType = ((ComponentCustomizer)sourceCustomizer).GetComponentType();
            ValueCopier.CopyValuesByName(componentType, sourceCustomizer, FindObjectOfType(componentType, true));
        }
    }

    public void GetBuildFromScene()
    {
        FieldInfo[] customizers = typeof(TromboneBuild).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        foreach (FieldInfo customizer in customizers)
        {
            object destinationCustomizer = customizer.GetValue(this);
            Type componentType = ((ComponentCustomizer)destinationCustomizer).GetComponentType();
            ValueCopier.CopyValuesByName(componentType, FindObjectOfType(componentType, true), destinationCustomizer);
        }
    }
}
