using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteAlways]
public class MainMenu : MenuUI
{
    [Header("UI Components")]
    public Button launchArcadeButton;
    public Button levelSelectButton;
    public Button leaderBoardsButton;
    public Button settingsButton;
    public Button exitButton;
    [Header("Events")]
    public UnityEvent onLaunchArcade;

    //private LevelLoader levelLoader;
    private LevelSelectionScreen levelSelection;
    private LeaderBoardScreen leaderBoards;
    private SettingsScreen settings;

    protected override void Awake()
    {
        base.Awake();
        //levelLoader = FindObjectOfType<LevelLoader>(true);
        levelSelection = FindObjectOfType<LevelSelectionScreen>(true);
        leaderBoards = FindObjectOfType<LeaderBoardScreen>(true);
        settings = FindObjectOfType<SettingsScreen>(true);
        //levelLoader.gameObject.SetActive(true);
    }

    //override protected void Start()
    //{
    //    if (Application.isPlaying && levelLoader != null) levelLoader.loadOnStart = false;
    //    base.Start();
    //}

    public override void ShowUI()
    {
        base.ShowUI();
        if (Application.isPlaying)
        {        
            // Activate buttons
            launchArcadeButton.onClick.AddListener(LaunchArcadeMode);
            levelSelectButton.onClick.AddListener(LaunchLevelSelection);
            leaderBoardsButton.onClick.AddListener(LaunchLeaderBoards);
            settingsButton.onClick.AddListener(LaunchSettings);
            exitButton.onClick.AddListener(ExitGame);
            // Deactivate level
            //if (levelLoader != null) levelLoader.enabled = false;
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            // Deactivate buttons
            launchArcadeButton.onClick.RemoveListener(LaunchArcadeMode);
            levelSelectButton.onClick.RemoveListener(LaunchLevelSelection);
            leaderBoardsButton.onClick.RemoveListener(LaunchLeaderBoards);
            settingsButton.onClick.RemoveListener(LaunchSettings);
            exitButton.onClick.RemoveListener(ExitGame);
        }
    }

    public void LaunchArcadeMode()
    {
        HideUI();
        onLaunchArcade.Invoke();       
    }

    public void LaunchLevelSelection()
    {
        if (levelSelection != null)
        {
            levelSelection.ShowUI();
            levelSelection.onGoBack.AddListener(ShowUI);
            HideUI();
        }
    }

    public void LaunchLeaderBoards()
    {
        if (leaderBoards != null)
        {
            leaderBoards.ShowUI();
            leaderBoards.onGoBack.AddListener(ShowUI);
            HideUI();
        }
    }

    public void LaunchSettings()
    {
        if (leaderBoards != null)
        {
            settings.ShowUI();
            settings.onGoBack.AddListener(ShowUI);
            HideUI();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
