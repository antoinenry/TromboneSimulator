using UnityEngine;
using System;
using System.Reflection;

[CreateAssetMenu(fileName = "NewTrombone", menuName = "Trombone Hero/Trombone Build")]
public class TromboneBuild : ScriptableObject
{
    public TromboneCoreCustomizer tromboneCore = new();
    public TromboneDisplayCustomizer tromboneDisplay = new();
    public TromboneAudioCustomizer tromboneAudio = new();
    public TromboneAutoCustomizer tromboneAuto = new();
    public MusicPlayerCustomizer musicPlayer = new();
    public AudioTrackGeneratorCustomizer audioTrackGenerator = new();
    public PerformanceJudgeCustomizer performanceJudge = new();
    public NoteCatcherCustomizer noteCatcher = new();

    public bool CreatedAtRuntime { get; private set; }

    private void Awake()
    {
        if (Application.isPlaying) CreatedAtRuntime = true;
    }

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
        FieldInfo[] customizers = typeof(TromboneBuild).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        foreach (FieldInfo customizer in customizers)
        {
            object destinationCustomizer = customizer.GetValue(this);
            Type componentType = ((ComponentCustomizer)destinationCustomizer).GetComponentType();
            ValueCopier.CopyValuesByName(componentType, FindObjectOfType(componentType, true), destinationCustomizer);
        }
    }
}
