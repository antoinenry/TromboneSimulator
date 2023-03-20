using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[CustomEditor(typeof(AudioTrackGenerator))]
public class AudioTrackGeneratorInspector : Editor
{
    private AudioTrackGenerator generator;
    private AudioTrackGenerator.ErrorType results;
    private AudioSource attachedAudioSource;

    private void OnEnable()
    {
        generator = target as AudioTrackGenerator;
        if (generator.OnGenerationProgress == null) generator.OnGenerationProgress = new UnityEvent<float>();
        generator.OnGenerationProgress.AddListener(OnGenerate);
        attachedAudioSource = generator.GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        generator.OnGenerationProgress.RemoveListener(OnGenerate);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Generate button
        if (GUILayout.Button("Generate Audio"))
            results = (target as AudioTrackGenerator).SampleTrack();
        if (results != AudioTrackGenerator.ErrorType.NoError)
            EditorGUILayout.HelpBox(results.ToString(), MessageType.Warning);
        // Progress display
        if (generator.TotalNoteCount > 0)
        {
            Rect controlRect = EditorGUILayout.GetControlRect();
            Rect fillRect = controlRect;
            float progress = (float)generator.GeneratedNoteCount / (float)generator.TotalNoteCount;
            fillRect.width *= progress;
            EditorGUI.DrawRect(controlRect, new Color(.1f, .1f, .1f));
            EditorGUI.DrawRect(fillRect, new Color(.1f, .5f, .6f, 1f));
            EditorGUI.LabelField(controlRect, (int)(progress * 100f) + "%", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Part " + generator.GeneratedPartCount + "/" + generator.PartCount, "Note " + generator.GeneratedNoteCount + "/" + generator.TotalNoteCount);
        }
        // Play button (if an AudioSource is attached to the target object)
        if (Application.isPlaying && attachedAudioSource != null && generator.AudioIsReady)
        {
            if (attachedAudioSource.isPlaying)
            {
                if (GUILayout.Button("Stop"))
                {
                    attachedAudioSource.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("Play"))
                {
                    attachedAudioSource.clip = generator.generatedAudio;
                    attachedAudioSource.Play();
                }
            }
        }
    }

    private void OnGenerate(float progress)
    {
        Repaint();
    }
}
