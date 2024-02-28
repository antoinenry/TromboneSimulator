using UnityEngine;
using System.Collections;

public class MenuMusic : MonoBehaviour
{   
    [Header("Components")]
    public MusicPlayer musicPlayer;
    public Trombone trombone;
    [Header("Music")]
    public SheetMusic music;
    public bool loop = true;
    [Header("Execution")]
    public MenuUI[] playInMenus;
    public bool enableTrombone = true;
    public bool setAutoTrombone = true;
    public TromboneAutoSettings tromboneAutoSettings;

    public bool IsPlaying { get; private set; }

    private TromboneAutoSettings restoreTromboneSettings;
    private AudioClip pregeneratedAudio;

    private void OnEnable()
    {
        foreach (MenuUI m in playInMenus)
        {
            m.onShowUI.AddListener(OnUIShown);
            m.onHideUI.AddListener(OnUIHidden);
        }
    }

    private void OnDisable()
    {
        foreach (MenuUI m in playInMenus)
        {
            m.onShowUI.RemoveListener(OnUIShown);
            m.onHideUI.RemoveListener(OnUIHidden);
        }
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
            if (pregeneratedAudio == null) StartCoroutine(PregenerateAudioCoroutine());
            else musicPlayer.LoadMusic(music, pregeneratedAudio, trombone.Sampler);
            musicPlayer.Play();
        }        
        // Setup trombone
        if (trombone != null)
        {
            if (enableTrombone) trombone.Unfreeze();
            else trombone.Freeze();
            restoreTromboneSettings = trombone.tromboneAuto.settings;
            if (setAutoTrombone) trombone.tromboneAuto.settings = tromboneAutoSettings;
        }
        IsPlaying = true;
    }

    public void Stop()
    {
        if (!IsPlaying) return;
        musicPlayer.Stop();
        IsPlaying = false;
        if (trombone != null) trombone.tromboneAuto.settings = restoreTromboneSettings;
    }

    private IEnumerator PregenerateAudioCoroutine()
    {
        musicPlayer.LoadMusic(music, playedInstrument:trombone.Sampler);
        yield return new WaitWhile(() => musicPlayer.IsLoading);
        pregeneratedAudio = AudioSampling.CloneAudioClip(musicPlayer.LoadedAudio, musicPlayer.LoadedAudio.name);
    }
}
