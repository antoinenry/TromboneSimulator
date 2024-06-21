using UnityEngine;
using System;
using System.Reflection;

[CreateAssetMenu(fileName = "NewTrombone", menuName = "Trombone Hero/Instruments/Trombone Customizer")]
public class TromboneCustomizer : ScriptableObject
{
    public TromboneCoreCustomizer tromboneCore = new();
    public TromboneDisplayCustomizer tromboneDisplay = new();
    public TromboneAudioCustomizer tromboneAudio = new();
    public TromboneAutoCustomizer tromboneAuto = new();
    public MusicPlayerCustomizer musicPlayer = new();
    public PerformanceJudgeCustomizer performanceJudge = new();

    public bool CreatedAtRuntime { get; private set; }

    private void Awake()
    {
        CreatedAtRuntime = Application.isPlaying;
    }

    public static void Copy(TromboneCustomizer source, TromboneCustomizer destination)
    {
        if (destination == null || source == null) return;
        FieldInfo[] customizers = typeof(TromboneCustomizer).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
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
        FieldInfo[] customizers = typeof(TromboneCustomizer).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        foreach (FieldInfo customizer in customizers)
        {
            object sourceCustomizer = customizer.GetValue(this);
            Type componentType = ((ComponentCustomizer)sourceCustomizer).GetComponentType();
            UnityEngine.Object destinationComponent = FindObjectOfType(componentType, true);
            if (destinationComponent == null) continue;
            ValueCopier.CopyValuesByName(componentType, sourceCustomizer, destinationComponent);
            typeof(ComponentCustomizer).InvokeMember("OnApplyToComponent", 
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, 
                null, sourceCustomizer, new object[1] {destinationComponent});
        }
    }

    public void GetBuildFromScene()
    {
        FieldInfo[] customizers = typeof(TromboneCustomizer).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        foreach (FieldInfo customizer in customizers)
        {
            object destinationCustomizer = customizer.GetValue(this);
            Type componentType = ((ComponentCustomizer)destinationCustomizer).GetComponentType();
            ValueCopier.CopyValuesByName(componentType, FindObjectOfType(componentType, true), destinationCustomizer);
        }
    }
}
