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
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        foreach (MenuUI m in playInMenus)
        {
            m.onShowUI.AddListener(OnUIShown);
            m.onHideUI.AddListener(OnUIHidden);
        }
        trombone?.onChangeBuild.AddListener(OnTromboneChange);
    }

    private void RemoveListeners()
    {
        foreach (MenuUI m in playInMenus)
        {
            m.onShowUI.RemoveListener(OnUIShown);
            m.onHideUI.RemoveListener(OnUIHidden);
        }
        trombone?.onChangeBuild.RemoveListener(OnTromboneChange);

    }

    private void OnUIShown()
    {
        Play();
    }

    private void OnUIHidden()
    {
        foreach (MenuUI m in playInMenus)
            if (m.IsVisible) return;
        Stop();
    }

    public void Play()
    {
        if (IsPlaying) return;
        float tempoStretch = 1f;
        // Setup trombone
        if (trombone != null)
        {
            if (enableTrombone) trombone.Unfreeze();
            else trombone.Freeze();
            if (pressureLock) LockTrombonePressure();
            if (trombone.CurrentBuild != null) tempoStretch = trombone.CurrentBuild.tempoStrecher;
        }
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
            // Play menu music
            musicPlayer.tempoStretch = tempoStretch;
            if (pregeneratedAudio == null) StartCoroutine(PregenerateAudioCoroutine());
            else musicPlayer.LoadMusic(music, pregeneratedAudio, trombone.Sampler);
            musicPlayer.Play();
        }
        IsPlaying = true;
    }

    public void Stop()
    {
        if (!IsPlaying) return;
        musicPlayer.Stop();
        IsPlaying = false;
    }

    public void OnTromboneChange()
    {
        if (trombone?.CurrentBuild?.tempoStrecher != musicPlayer?.tempoStretch)
        {            
            Stop();
            pregeneratedAudio = null;
            Play();
        }
    }

    private IEnumerator PregenerateAudioCoroutine()
    {
        musicPlayer.LoadMusic(music, playedInstrument:trombone.Sampler);
        yield return new WaitWhile(() => musicPlayer.IsLoading);
        pregeneratedAudio = AudioSampling.CloneAudioClip(musicPlayer.LoadedAudio, musicPlayer.LoadedAudio.name);
    }

    private void LockTrombonePressure()
    {
        if (trombone?.tromboneAuto == null) return;
        TromboneAutoSettings autoSettings = trombone.tromboneAuto.settings;
        autoSettings.pressureLock = TromboneAutoSettings.LockConditions.AutoBlows;
        trombone.tromboneAuto.settings = autoSettings;
    }
}
