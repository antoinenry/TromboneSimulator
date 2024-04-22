using UnityEngine;

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

    public bool IsPlaying { get; private set; }

    private void OnEnable()
    {
        AddMenuListeners();
    }

    private void OnDisable()
    {
        RemoveMenuListeners();
    }

    private void Update()
    {
        TromboneSetup();
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
        musicPlayer?.audioGenerator?.OnGenerationProgress.AddListener(OnLoadMusic);
        TromboneSetup();
        StartPlaying();
    }

    private void OnUIHidden()
    {
        musicPlayer?.audioGenerator?.OnGenerationProgress.RemoveListener(OnLoadMusic);
        foreach (MenuUI m in playInMenus) if (m.IsVisible) return;
        StopPlaying();
    }

    private void OnLoadMusic(float progress)
    {
        if (progress < 1f) StopPlaying();
        else
        {
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
            musicPlayer.LoadMusic(music, playedInstrument:trombone.Sampler);
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

    private void TromboneSetup()
    {
        if (trombone == null) return;
        if (enableTrombone) trombone.Unfreeze();
        else trombone.Freeze();
        // Special auto setting (lock pressure) for prettier autoplay in menu music
        if (trombone.tromboneAuto != null && trombone.tromboneAuto.autoSettings.blowControl != TromboneAutoSettings.ControlConditions.Never)
        {
            TromboneAutoSettings autoSettings = trombone.tromboneAuto.autoSettings;
            autoSettings.pressureLock = TromboneAutoSettings.LockConditions.AutoBlows;
            trombone.tromboneAuto.autoSettings = autoSettings;
        }
    }
}
