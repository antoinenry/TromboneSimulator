using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SamplerInstrument))]
[CanEditMultipleObjects]
public class SamplerInstrumentInspector : Editor
{
    private SamplerInstrument instrument;
    private AudioClip instrumentAudio;
    private float lowTone;
    private float highTone;

    public static float attackThreshold = -.5f;

    private void OnEnable()
    {
        instrument = (target as SamplerInstrument);
        instrumentAudio = instrument.fullAudio;
        if (instrument.tones != null && instrument.tones.Length >= 2)
        {
            lowTone = instrument.LowestTone;
            highTone = instrument.HighestTone;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (targets.Length > 1) return;
        // Tool to set tones
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Auto set tones", EditorStyles.boldLabel);
        // Sample lengths
        lowTone = ToneAttributeDrawer.GUIToneField("Low tone", lowTone, !instrument.drumkit);
        highTone = ToneAttributeDrawer.GUIToneField("High tone", highTone, !instrument.drumkit);
        if (GUILayout.Button("Set"))
        {
            Undo.RecordObject(target, "Set tones");
            (target as SamplerInstrument).SetTones(lowTone, highTone);
        }
        // Attack
        attackThreshold = EditorGUILayout.Slider("Attack threshold", attackThreshold, 0f, 1f);
        if (GUILayout.Button("Set attacks"))
        {
            Undo.RecordObject(target, "Set attacks");
            (target as SamplerInstrument).GuessToneAttacks(attackThreshold);
        }        
        EditorGUILayout.EndVertical();
        // Auto set tones on audio file change
        if (InstrumentDictionary.Current != null)
        {
            if (instrumentAudio != instrument.fullAudio)
            {
                instrumentAudio = instrument.fullAudio;
                if (instrumentAudio != null)
                {
                    if (instrument.TryParseFileName(instrumentAudio.name, out float findLowTone, out float findHighTone))
                    {
                        lowTone = findLowTone;
                        highTone = findHighTone;
                    }
                }
            }
        }
        else
            EditorGUILayout.HelpBox("No current instrument dictionary.", MessageType.Info);
    }
}