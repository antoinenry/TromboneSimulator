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
        TromboneSetup();
        PlayMusic();
        trombone?.onChangeBuild.AddListener(OnTromboneChange);
    }

    private void OnUIHidden()
    {
        foreach (MenuUI m in playInMenus) if (m.IsVisible) return;
        trombone?.onChangeBuild.RemoveListener(OnTromboneChange);
        StopMusic();
    }

    public void PlayMusic()
    {
        if (IsPlaying) return;
        // Setup background music
        if (musicPlayer != null)
        {
            // Reset player settings
            musicPlayer.enabled = true;
            musicPlayer.Stop();
            musicPlayer.loop = loop;
            musicPlayer.playingSpeed = 1f;
            // Turn off metronome
            musicPlayer.metronome.click = false;
            // Generate menu music if needed
            float tempoStretch = trombone?.CurrentBuild != null ? trombone.CurrentBuild.tempoStrecher : 1f;
            if (pregeneratedAudio == null || musicPlayer.tempoStretch != tempoStretch)
            {
                musicPlayer.tempoStretch = tempoStretch;
                StartCoroutine(PregenerateAudioCoroutine());
            }
            else musicPlayer.LoadMusic(music, pregeneratedAudio, trombone.Sampler);
            // Play
            musicPlayer.Play();
            IsPlaying = true;
        }
        else IsPlaying = false;
    }

    public void StopMusic()
    {
        if (!IsPlaying) return;
        musicPlayer.Stop();
        IsPlaying = false;
    }

    public void OnTromboneChange()
    {
        // Reload music if needed (changes in tempo, orchestra...)
        if (trombone?.CurrentBuild?.tempoStrecher != musicPlayer?.tempoStretch)
        {            
            StopMusic();
            PlayMusic();
        }
        // Setup trombone
        TromboneSetup();
    }

    private IEnumerator PregenerateAudioCoroutine()
    {
        musicPlayer.LoadMusic(music, playedInstrument:trombone.Sampler);
        yield return new WaitWhile(() => musicPlayer.IsLoading);
        pregeneratedAudio = AudioSampling.CloneAudioClip(musicPlayer.LoadedAudio, musicPlayer.LoadedAudio.name);
    }

    private void TromboneSetup()
    {
        if (trombone == null) return;
        if (enableTrombone) trombone.Unfreeze();
        else trombone.Freeze();
        if (trombone.tromboneAuto != null && pressureLock)
        {
            TromboneAutoSettings autoSettings = trombone.tromboneAuto.settings;
            autoSettings.pressureLock = TromboneAutoSettings.LockConditions.AutoBlows;
            trombone.tromboneAuto.settings = autoSettings;
        }
    }
}
