using UnityEditor;
using UnityEngine;
using static InstrumentDictionary;

[CustomEditor(typeof(SamplerInstrument))]
[CanEditMultipleObjects]
public class SamplerInstrumentInspector : Editor
{
    private SamplerInstrument instrument;
    private AudioClip instrumentAudio;
    private float lowTone;
    private float highTone;
    private DrumHitInfo[] drumHits;
    private int silenceSamples = 1000;
    private float silenceThreshold = 0f;

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
        drumHits = Current?.drumHits;
        if (drumHits == null) drumHits = new DrumHitInfo[0];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (targets.Length > 1) return;
        // Tool to set tones
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Auto set tones", EditorStyles.boldLabel);
        // Sample lengths
        if (instrument.drumkit)
        {
            silenceSamples = EditorGUILayout.IntField("Min sample spacing", silenceSamples);
            silenceThreshold = EditorGUILayout.FloatField("Silence threshold", silenceThreshold);
            if (GUILayout.Button("Auto-set hits"))
            {
                Undo.RecordObject(target, "Auto-set hits hits");
                (target as SamplerInstrument).AutoSetHits(drumHits, silenceSamples, silenceThreshold);
            }
        }
        else
        {
            lowTone = ToneAttributeDrawer.GUIToneField("Low tone", lowTone, !instrument.drumkit);
            highTone = ToneAttributeDrawer.GUIToneField("High tone", highTone, !instrument.drumkit);
            if (GUILayout.Button("Auto-set tones"))
            {
                Undo.RecordObject(target, "Auto-set hits tones");
                (target as SamplerInstrument).AutoSetTones(lowTone, highTone);
            }
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
}