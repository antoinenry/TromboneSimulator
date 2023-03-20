using UnityEngine;
using System.Collections;

public class MenuMusic : MonoBehaviour
{   
    [Header("Components")]
    public MusicPlayer musicPlayer;
    public Trombone trombone;
    [Header("Music")]
    public SheetMusic music;
    public AudioClip preloadedAudio;
    public bool loop = true;
    [Header("Execution")]
    public MenuUI[] playInMenus;
    public bool enableTrombone = true;
    public bool setAutoTrombone = true;
    public TromboneAutoSettings tromboneAutoSettings;

    public bool IsPlaying { get; private set; }

    private TromboneAutoSettings restoreTromboneSettings;

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
            musicPlayer.enabled = true;
            musicPlayer.Stop();
            musicPlayer.loop = loop;
            musicPlayer.playingSpeed = 1f;
            if (preloadedAudio == null) StartCoroutine(SetPreloadedAudioCoroutine());
            else musicPlayer.LoadMusic(music, preloadedAudio, trombone.Sampler);
            musicPlayer.Play();
        }        
        // Setup trombone
        if (trombone != null)
        {
            trombone.enabled = enableTrombone;
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

    private IEnumerator SetPreloadedAudioCoroutine()
    {
        musicPlayer.LoadMusic(music, null, trombone.Sampler);
        preloadedAudio = musicPlayer.backingSource.clip;
        while (musicPlayer.IsLoading) yield return null;
        if (preloadedAudio != musicPlayer.backingSource.clip) preloadedAudio = null;
    }
}
