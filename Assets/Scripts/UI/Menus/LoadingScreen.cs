using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LoadingScreen : MenuUI
{
    [Header("UI Components")]
    public Slider loadingSlider;
    public Transform loadingOrchestraLayout;
    public TextMeshProUGUI loadingMessage;
    [Header("Content")]
    public bool showOrchestra = true;
    public string defaultLoadingText = "LOADINGUE";
    public Color orchestraLowTint = Color.black;
    public Color orchestraHighTint = Color.white;

    private AudioTrackGenerator trackGenerator;
    //private HandCursor cursor;
    private Image[] loadingOrchestra;

    protected override void Awake()
    {
        base.Awake();
        trackGenerator = FindObjectOfType<AudioTrackGenerator>(true);
        //cursor = FindObjectOfType<HandCursor>(true);
        if (loadingOrchestraLayout != null) loadingOrchestra = loadingOrchestraLayout.GetComponentsInChildren<Image>(true);
    }

    private void OnEnable()
    {
        if (trackGenerator != null) trackGenerator.OnGenerationProgress.AddListener(OnLoadMusic);
    }

    private void OnDisable()
    {
        if (trackGenerator != null) trackGenerator.OnGenerationProgress.RemoveListener(OnLoadMusic);
    }

    private void OnLoadMusic(float progress)
    {
        if (progress < 1f)
        {
            // Don't show orchestra when loading a menu screen
            showOrchestra = VisibleMenuCount == 1;
            if (IsVisible == false)
            {
                SetOrchestraLayout(trackGenerator.music.PartNames);
                ResetOrchestraLoadProgress();
                if (cursor != null) cursor.cursorState &= ~HandCursor.CursorState.Visible;
                ShowUI();
            }
            if (loadingSlider != null) loadingSlider.value = progress * loadingSlider.maxValue;
            SetOrchestraLoadProgress(trackGenerator.CurrentPart, trackGenerator.CurrentPartGeneratedNotes, trackGenerator.CurrentPartLength);
        }
        else
        {
            if (IsVisible == true)
            {
                SetOrchestraLayout(null);
                if (cursor != null) cursor.cursorState |= HandCursor.CursorState.Visible;
                HideUI();
            }
        }
    }

    private void SetOrchestraLayout(string[] instrumentNames)
    {
        if (loadingOrchestra != null)
        {
            foreach (Image member in loadingOrchestra)
            {
                bool activeMember = false;
                if (showOrchestra == true && instrumentNames != null)
                    activeMember = (Array.FindIndex(instrumentNames, i => InstrumentDictionary.SameCurrentInstruments(i, member.name)) != -1);
                member.gameObject.SetActive(activeMember);
            }
        }
    }

    private void ResetOrchestraLoadProgress()
    {
        if (loadingOrchestra != null)
        {
            foreach (Image member in loadingOrchestra)
                member.color = orchestraLowTint;
        }
    }

    private void SetOrchestraLoadProgress(string instrumentName, int generatedNotes, int partLength)
    {
        if (loadingOrchestra != null)
        {
            float progress = (float)generatedNotes / partLength;
            Image member = Array.Find(loadingOrchestra, m => InstrumentDictionary.SameCurrentInstruments(instrumentName, m.name));
            if (member != null)
            {
                member.gameObject.SetActive(showOrchestra);
                member.color = Color.Lerp(orchestraLowTint, orchestraHighTint, progress);
            }
        }
    }
}
