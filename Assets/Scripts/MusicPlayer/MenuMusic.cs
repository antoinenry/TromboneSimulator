using UnityEngine;
using System.Collections;

public class MenuMusic : MonoBehaviour
{   
    [Header("Components")]
    public MusicPlayer musicPlayer;
    public TromboneCore trombone;
    [Header("Music")]
    public SheetMusic music;
    public bool loop = true;
    [Header("Execution")]
    public MenuUI[] playInMenus;
    public bool enableTrombone = true;
    public bool pressureLock = true;

    public bool IsPlaying { get; private set; }

    private AudioClip pregeneratedAudio;

    private void OnEnable()
    {
        AddMenuListeners();
    }

    private void OnDisable()
    {
        RemoveMenuListeners();
    }

    private void AddMenuListeners()
    {
        foreach (MenuUI m in playInMenus)
        {
            m.onShowUI.AddListener(OnUIShown);
            m.onHideUI.AddListener(OnUIHidden);
        }
    }

    private void RemoveMenuListeners()
    {
        foreach (MenuUI m in playInMenus)
        {
            m.onShowUI.RemoveListener(OnUIShown);
            m.onHideUI.RemoveListener(OnUIHidden);
        }
    }

    private void OnUIShown()
    {
        musicPlayer?.backingGenerator?.OnGenerationProgress.AddListener(OnLoadMusic);
        TromboneSetup();
        StartPlaying();
    }

    private void OnUIHidden()
    {
        musicPlayer?.backingGenerator?.OnGenerationProgress.RemoveListener(OnLoadMusic);
        foreach (MenuUI m in playInMenus) if (m.IsVisible) return;
        StopPlaying();
    }

    private void OnLoadMusic(float progress)
    {
        if (progress < 1f) StopPlaying();
        else
        {
            AudioClip originalAudio = musicPlayer.backingGenerator?.generatedAudio;
            pregeneratedAudio = AudioSampling.CloneAudioClip(originalAudio, originalAudio?.name);
            TromboneSetup();
            StartPlaying();
        }
    }

    public void StartPlaying()
    {
        if (IsPlaying) return;
        // Setup background music
        if (musicPlayer != null)
        {
            // Reset player settings
            musicPlayer.enabled = true;
            musicPlayer.Stop();
            musicPlayer.loop = loop;
            // Turn off metronome
            musicPlayer.metronome.click = false;
            // Load music
            musicPlayer.LoadMusic(music, pregeneratedAudio, playedInstrument:trombone.Sampler);
            // Play
            musicPlayer.Play();
            IsPlaying = true;
        }
        else IsPlaying = false;
    }

    public void StopPlaying()
    {
        if (!IsPlaying) return;
        musicPlayer.Stop();
        IsPlaying = false;
    }

    private IEnumerator PregenerateAudioCoroutine()
    {
        
        yield return new WaitWhile(() => musicPlayer.IsLoading);
        //pregeneratedAudio = AudioSampling.CloneAudioClip(musicPlayer.LoadedAudio, musicPlayer.LoadedAudio.name);
    }

    private void TromboneSetup()
    {
        if (trombone == null) return;
        if (enableTrombone) trombone.Unfreeze();
        else trombone.Freeze();
        if (trombone.tromboneAuto != null && pressureLock)
        {
            TromboneAutoSettings autoSettings = trombone.tromboneAuto.autoSettings;
            autoSettings.pressureLock = TromboneAutoSettings.LockConditions.AutoBlows;
            trombone.tromboneAuto.autoSettings = autoSettings;
        }
    }
}
