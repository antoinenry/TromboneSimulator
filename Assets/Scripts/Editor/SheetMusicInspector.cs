using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SheetMusic))]
public class SheetMusicInspector : Editor
{
    private SheetMusic sheetMusic;
    private string MIDIDirectory;
    private string MIDIPath;
    private MIDIExtractor.ErrorType extractResult;
    private bool displayCheckResult;
    private float[] undefinedDrumTones;
    private float[] outOfRangeTones;
    private string[] outOfRangeInstrumentNames;
    private int[] voiceCount;
    private int transposeInstrumentIndex;
    private float transposeTones;

    private void OnEnable()
    {
        sheetMusic = target as SheetMusic;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Midi extraction panel
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Extract MIDI File", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        MIDIPath = EditorGUILayout.TextField(MIDIPath);
        bool openFile = GUILayout.Button("...", GUILayout.Width(30f));
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Extract MIDI"))
        {
            Undo.RecordObject(target, "Extract MIDI File");
            ExtractMIDI();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndVertical();
        // File explorer
        if (openFile) MIDIPath = EditorUtility.OpenFilePanel("Open MIDI File", MIDIDirectory, "mid");
        // Midi extraction messages
        switch (extractResult)
        {
            case MIDIExtractor.ErrorType.NoFile:
                EditorGUILayout.HelpBox("MIDI file not found", MessageType.Warning);
                break;
            case MIDIExtractor.ErrorType.NoTempoTrack:
                EditorGUILayout.HelpBox("No tempo track", MessageType.Warning);
                break;
            case MIDIExtractor.ErrorType.SeveralTempoTracks:
                EditorGUILayout.HelpBox("Too many tempo tracks", MessageType.Warning);
                break;
            case MIDIExtractor.ErrorType.OpenNote:
                EditorGUILayout.HelpBox("Incomplete note", MessageType.Warning);
                break;
        }
        // Transposition tool
        EditorGUILayout.LabelField("Transpose part", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        List<string> partNames = sheetMusic.PartCount > 0 ? new List<string>(sheetMusic.PartNames) : new List<string>();
        partNames.Add("All Parts");
        transposeInstrumentIndex = EditorGUILayout.Popup(transposeInstrumentIndex, partNames.ToArray());
        //transposeInstrumentName = EditorGUILayout.TextField("Transpose", transposeInstrumentName);
        transposeTones = EditorGUILayout.FloatField(transposeTones);
        if (GUILayout.Button("Transpose"))
        {
            Undo.RecordObject(target, "Transpose");
            if (transposeInstrumentIndex < partNames.Count - 1) 
                sheetMusic.TransposePart(transposeInstrumentIndex, transposeTones);
            else
                sheetMusic.Transpose(transposeTones);
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
        // Check tools
        EditorGUILayout.LabelField("Check notes", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        // Instrument tone check
        if (GUILayout.Button("Check instrument ranges"))
        {
            displayCheckResult = true;
            sheetMusic.FindOutOfRangeTones(out outOfRangeInstrumentNames, out outOfRangeTones);
        }
        // Drumhit tone check
        if (GUILayout.Button("Check drum part"))
        {
            displayCheckResult = true;
            sheetMusic.FindUnknownDrumHits(out undefinedDrumTones);
        }
        // Voice count check
        if (GUILayout.Button("Check voices"))
        {
            displayCheckResult = true;
            voiceCount = new int[sheetMusic.PartCount];
            for (int p = 0; p < sheetMusic.PartCount; p++)
                voiceCount[p] = sheetMusic.GetVoiceCount(p);
        }
        EditorGUILayout.EndHorizontal();
        // Results from checks
        if (displayCheckResult)
        {
            bool noResults = true;
            if (outOfRangeTones != null && outOfRangeTones.Length > 0)
            {
                noResults = false;
                EditorGUILayout.HelpBox("Out of range values:", MessageType.Warning);
                int outOfRangeCount = outOfRangeTones.Length;
                string outOfRangeValues = string.Empty;
                for (int i = 0; i < outOfRangeCount; i++)
                    outOfRangeValues += outOfRangeInstrumentNames[i] + "(" + outOfRangeTones[i].ToString() + "); ";
                EditorGUILayout.HelpBox(outOfRangeValues, MessageType.Info);
            }
            if (undefinedDrumTones != null && undefinedDrumTones.Length > 0)
            {
                noResults = false;
                EditorGUILayout.HelpBox("Unknown drumhit values:", MessageType.Warning);
                string unknownValues = string.Empty;
                foreach (float t in undefinedDrumTones) unknownValues += t.ToString() + "; ";
                EditorGUILayout.HelpBox(unknownValues, MessageType.Info);
            }
            if (voiceCount != null)
            {
                for (int i = 0; i < sheetMusic.PartCount && i < voiceCount.Length; i++)
                {
                    if (InstrumentDictionary.IsCurrentDrums(partNames[i])) continue;
                    int count = voiceCount[i];
                    if (count == 0)
                    {
                        noResults = false;
                        EditorGUILayout.HelpBox(partNames[i] + ": " + count.ToString() + " voices.", MessageType.Warning);
                    }
                    else if (count > 1)
                    {
                        noResults = false;
                        EditorGUILayout.HelpBox(partNames[i] + ": " + count.ToString() + " voices.", MessageType.Info);
                    }
                }
            }
            if (noResults)
                EditorGUILayout.HelpBox("No results.", MessageType.Info);
        }        
    }

    private void ExtractMIDI()
    {
        MIDIExtractor extractor = new MIDIExtractor();
        extractor.TryExtractFile(MIDIPath, target as SheetMusic);
        extractResult = extractor.result;
    }
}
