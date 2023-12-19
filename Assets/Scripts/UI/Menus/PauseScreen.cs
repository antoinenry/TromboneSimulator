using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PauseScreen : MenuUI
{
    public Button stopButton;
    public Button playButton;
    public Button settingsButton;
    public TextMeshProUGUI titleLine1;
    public TextMeshProUGUI titleLine2;

    public UnityEvent onUnpause;
    public UnityEvent onQuit;
    public UnityEvent onOpenSettings;

    //private SettingsScreen UISettingsScreen;
    //private LevelLoader levelLoader;
    //private MusicPlayer musicPlayer;

    protected override void Awake()
    {
        base.Awake();
        UIPause = this;
    }

    override protected void Start()
    {
        //UISettingsScreen = FindObjectOfType<SettingsScreen>(true);
        //musicPlayer = FindObjectOfType<MusicPlayer>(true);
        //levelLoader = FindObjectOfType<LevelLoader>(true);
        base.Start();
    }

    override public void ShowUI()
    {
        //if (musicPlayer != null)// && musicPlayer.loadedMusic != null)
        //{
        //    //titleLine1.text = musicPlayer.loadedMusic.artist;
        //    //titleLine2.text = musicPlayer.loadedMusic.title;
        //}
        //else
        //{
        //    titleLine1.text = "";
        //    titleLine2.text = "";
        //}
        base.ShowUI();
        if (Application.isPlaying)
        {
            stopButton.onClick.AddListener(Quit);
            playButton.onClick.AddListener(Unpause);
            settingsButton.onClick.AddListener(OpenSettings);
        }
        // Detach hand cursor from trombone
        if (cursor != null) cursor.cursorState &= ~HandCursor.CursorState.Trombone;
    }

    override public void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            // Disable buttons
            stopButton.onClick.RemoveListener(Quit);
            playButton.onClick.RemoveListener(Unpause);
            settingsButton.onClick.RemoveListener(OpenSettings);
            // Remove all listeners
            onUnpause.RemoveAllListeners();
            onQuit.RemoveAllListeners();
            onOpenSettings.RemoveAllListeners();
        }
    }

    public void Unpause()
    {
        onUnpause.Invoke();
        HideUI();
    }

    private void Quit()
    {
        onQuit.Invoke();
        HideUI();
    }

    private void OpenSettings()
    {
        onOpenSettings.Invoke();
        //UISettingsScreen.ShowUI();
        //UISettingsScreen.onGoBack.AddListener(ShowUI);
        //HideUI();
    }
}
