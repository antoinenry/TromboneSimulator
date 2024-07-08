using UnityEngine;

public class MenuMusic : MonoBehaviour
{       
    [Header("Music")]
    public SheetMusic music;
    public bool loop = true;
    [Header("Execution")]
    public bool trombonePlay = true;

    private MenuUI menu;
    private MusicPlayer musicPlayer;
    private TromboneCore trombone;

    public static MenuUI playingMenu;

    private void Awake()
    {
        menu = GetComponent<MenuUI>();
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        trombone = FindObjectOfType<TromboneCore>(true);
    }

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
        menu.onShowUI.AddListener(OnUIShown);
        menu.onHideUI.AddListener(OnUIHidden);
    }

    private void RemoveMenuListeners()
    {
        menu.onShowUI.RemoveListener(OnUIShown);
        menu.onHideUI.RemoveListener(OnUIHidden);
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
        if (playingMenu == menu) StopPlaying();
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
        playingMenu = menu;
        // Setup background music
        if (musicPlayer != null)
        {
            musicPlayer.enabled = true;
            musicPlayer.loop = loop;
            musicPlayer.metronome.click = false;
            musicPlayer.LoadMusic(music, playedInstrument:trombone.Sampler);
            musicPlayer.Play();
        }
    }

    public void StopPlaying()
    {
        if (playingMenu == menu) playingMenu = null;
        musicPlayer.Stop();
    }

    public void TromboneSetup()
    {
        if (trombone == null) return;
        if (trombonePlay) trombone.Unfreeze();
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
