using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LoadingScreen : MenuUI
{
    [Header("UI Components")]
    public Slider loadingSlider;
    public Transform loadingOrchestraLayout;
    public Image background;
    [Header("Content")]
    public bool showOrchestra = true;
    public bool showAsPanel = true;
    public string defaultLoadingText = "LOADINGUE";
    public Color orchestraLowTint = Color.black;
    public Color orchestraHighTint = Color.white;

    private AudioTrackGenerator trackGenerator;    
    private Image[] loadingOrchestra;
    private UILoadingSFX loadingSFX;

    protected override void Awake()
    {
        base.Awake();
        trackGenerator = FindObjectOfType<AudioTrackGenerator>(true);
        loadingOrchestra = loadingOrchestraLayout?.GetComponentsInChildren<Image>(true);
        loadingSFX = GetComponent<UILoadingSFX>();
    }

    private void OnEnable()
    {
        if (trackGenerator != null) trackGenerator.onGenerationProgress.AddListener(OnLoadMusic);
    }

    private void OnDisable()
    {
        if (trackGenerator != null) trackGenerator.onGenerationProgress.RemoveListener(OnLoadMusic);
    }

    protected override void Update()
    {
        base.Update();
        loadingOrchestraLayout?.gameObject?.SetActive(showOrchestra && !showAsPanel);
        background?.gameObject?.SetActive(!showAsPanel);
    }

    private void OnLoadMusic(float progress)
    {
        // Loading
        if (progress < 1f)
        {
            onLoadingScreenVisible?.Invoke(this, true);
            if (IsVisible == false)
            {
                SetOrchestraLayout(trackGenerator.music.PartNames);
                ResetOrchestraLoadProgress();
                if (cursor != null) cursor.State &= ~HandCursor.CursorState.Visible;
                ShowUI();
            }
            if (loadingSlider != null) loadingSlider.value = progress * loadingSlider.maxValue;
            SetOrchestraLoadProgress(trackGenerator.CurrentPart, trackGenerator.CurrentPartGeneratedNotes, trackGenerator.CurrentPartLength);
        }
        // Complete
        else
        {
            if (IsVisible == true)
            {
                SetOrchestraLayout(null);
                if (cursor != null) cursor.State |= HandCursor.CursorState.Visible;
                HideUI();
            }
            onLoadingScreenVisible?.Invoke(this, false);
        }
        // SFX
        loadingSFX?.OnLoadingProgress(progress);
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
