using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public enum Mode { ARCADE, ONE_LEVEL }

    public bool loadOnAwake = false;
    public bool cheatMode = false;
    [Header("Game")]
    public GameState gameState;
    public Mode currentMode;
    public int startContinues = 3;
    [Header("Transitions")]
    public float minimumCountdownStepDuration = 1.5f;
    public int startCountdownValue = 3;
    public float levelSetupTransitionDuration = .5f;
    public float gameOverTransitionDuration = 2f;

    public Level LoadedLevel { get; private set; }
    public AudioClip LoadedLevelAudio { get; private set; }

    private MusicPlayer musicPlayer;
    private NoteCatcher noteCatcher;
    private NoteSpawner noteSpawner;
    private PerformanceJudge perfJudge;
    private Trombone trombone;
    private LevelGUI GUI;
    private MainMenu UIMainMenu;
    private LevelSelectionScreen UILevelSelection;
    private LeaderBoardScreen UILeaderboard;
    private PauseScreen UIPause;
    private ScoreScreen UIScore;
    private GameOverScreen UIGameOver;
    private NewHighscoreScreen UIHighScore;

    private void Awake()
    {
        // Find components
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        noteCatcher = FindObjectOfType<NoteCatcher>(true);
        noteSpawner = FindObjectOfType<NoteSpawner>(true);
        perfJudge = FindObjectOfType<PerformanceJudge>(true);
        trombone = FindObjectOfType<Trombone>(true);
        // Find UI components
        GUI = FindObjectOfType<LevelGUI>(true);
        UIMainMenu = FindObjectOfType<MainMenu>(true);
        UILevelSelection = FindObjectOfType<LevelSelectionScreen>(true);
        UILeaderboard = FindObjectOfType<LeaderBoardScreen>(true);
        UIPause = FindObjectOfType<PauseScreen>(true);
        UIScore = FindObjectOfType<ScoreScreen>(true);
        UIGameOver = FindObjectOfType<GameOverScreen>(true);
        UIHighScore = FindObjectOfType<NewHighscoreScreen>(true);
        // Clear level load
        Unload();
        // Initialize savestate
        if (gameState != null)
        {
            bool loadSuccess = gameState.TryLoadState();
            if (!loadSuccess) gameState.SaveState();
            // Load on awake
            if (loadOnAwake) LoadLevel(currentMode, gameState.CurrentLevelNumber);
        }
    }

    private void OnEnable()
    {
        GUI.onPressPause.AddListener(PauseLevel);
    }

    private void OnDisable()
    {
        GUI.onPressPause.RemoveListener(PauseLevel);
    }

    private void Start()
    {
        // Permanents listeners (even when LevelLoader is disabled)
        gameState.onChangeGameSettings.AddListener(ApplySettings);
        UIMainMenu.onLaunchArcade.AddListener(LaunchArcadeMode);
        UILevelSelection.onShowUI.AddListener(UpdateLevelSelection);
        UILevelSelection.onSelectLevel.AddListener(LaunchOneLevelMode);
        UILeaderboard.onShowUI.AddListener(UpdateLeaderBoard);
    }

    private void OnDestroy()
    {
        // Remove permanents listeners
        gameState.onChangeGameSettings.RemoveListener(ApplySettings);
        UIMainMenu.onLaunchArcade.RemoveListener(LaunchArcadeMode);
        UILevelSelection.onShowUI.RemoveListener(UpdateLevelSelection);
        UILevelSelection.onSelectLevel.RemoveListener(LaunchOneLevelMode);
        UILeaderboard.onShowUI.RemoveListener(UpdateLeaderBoard);
    }

    private void Update()
    {
        // Cheat mode
        if (MenuUI.VisibleMenuCount == 0 && cheatMode == true)
            CheatKeys();
        // Update GUI
        if (musicPlayer != null)
            GUI.SetTimeBar(musicPlayer.playTime, musicPlayer.MusicDuration);
    }

    private void EnablePerformanceJudge()
    {
        perfJudge.onScore.AddListener(OnScore);
        perfJudge.onHealth.AddListener(OnHealth);
        perfJudge.onDance.AddListener(OnDance);
        perfJudge.onCorrectNote.AddListener(OnNotePerformanceCorrect);
        perfJudge.onWrongNote.AddListener(OnNotePerformanceWrong);
        perfJudge.onMissNote.AddListener(OnNotePerformanceMiss);
        perfJudge.onNotePerformanceEnd.AddListener(OnNotePerformanceEnd);
        perfJudge.enabled = true;
    }

    private void DisablePerformanceJudge()
    {
        perfJudge.onScore.RemoveListener(OnScore);
        perfJudge.onHealth.RemoveListener(OnHealth);
        perfJudge.onDance.RemoveListener(OnDance);
        perfJudge.onCorrectNote.RemoveListener(OnNotePerformanceCorrect);
        perfJudge.onWrongNote.RemoveListener(OnNotePerformanceWrong);
        perfJudge.onMissNote.RemoveListener(OnNotePerformanceMiss);
        perfJudge.onNotePerformanceEnd.RemoveListener(OnNotePerformanceEnd);
        perfJudge.enabled = false;
    }

    public void LaunchArcadeMode()
    {
        gameState.continues = startContinues;
        gameState.ClearCurrentScore();
        LoadLevel(Mode.ARCADE, 1);
    }

    public void LaunchOneLevelMode(int levelNumber)
    {
        gameState.continues = 0;
        gameState.ClearCurrentScore();
        LoadLevel(Mode.ONE_LEVEL, levelNumber);
    }

    public void Unload()
    {
        LoadedLevel = null;
        LoadedLevelAudio = null;
        // Stop GUI
        GUI.GUIActive = false;
        // Stop music
        musicPlayer.Stop();
        musicPlayer.UnloadMusic();
        musicPlayer.onMusicEnd.RemoveListener(OnLevelEnd);
        // Stop trombone
        trombone.enabled = false;
        // Stop performance judge
        DisablePerformanceJudge();
        // Stop note spawner
        noteSpawner.enabled = false;
        // Stop note catcher
        noteCatcher.enabled = false;
    }

    public void LoadLevel(Mode mode, int levelNumber)
    {
        currentMode = mode;
        gameState.CurrentLevelNumber = levelNumber;
        // Get level
        LoadedLevel = gameState.CurrentLevel;
        // Music setup
        musicPlayer.Stop();
        musicPlayer.LoadMusic(LoadedLevel.music, LoadedLevelAudio, trombone.Sampler);
        LoadedLevelAudio = musicPlayer.LoadedAudio;
        musicPlayer.ApplySettings(gameState.Settings);
        musicPlayer.loop = false;
        musicPlayer.onMusicEnd.AddListener(OnLevelEnd);
        // Trombone setup
        trombone.enabled = true;
        trombone.LoadBuild();
        trombone.ResetTrombone();
        // NoteSpawner setup
        noteSpawner.enabled = true;
        // Note catcher setup
        noteCatcher.trombone = trombone;
        noteCatcher.enabled = true;
        // Judge setup
        EnablePerformanceJudge();
        if (trombone.Sampler != null) perfJudge.LevelSetup(LoadedLevel.music, trombone.Sampler.instrumentName);
        // GUI Setup
        GUI.GUIActive = true;
        // Level start sequence
        StartCoroutine(LevelStartSequence());
    }

    public void RestartLevel()
    {
        DisablePerformanceJudge();
        LoadLevel(currentMode, gameState.CurrentLevelNumber);
    }

    public void ApplySettings(GameSettingsInfo settings)
    {
        musicPlayer.ApplySettings(settings);
    }

    public void PauseLevel()
    {
        musicPlayer.Pause(true);
        trombone.enabled = false;
        UIPause.ShowUI();
        UIPause.onUnpause.AddListener(UnpauseLevel);
        UIPause.onQuit.AddListener(QuitLevel);
        StopCoroutine(LevelUnpauseSequence());
    }

    public void UnpauseLevel()
    {
        UIPause.HideUI();
        trombone.enabled = true;
        UIPause.onUnpause.RemoveListener(UnpauseLevel);
        UIPause.onQuit.RemoveListener(QuitLevel);
        //musicPlayer.Play(true);
        // Unpause sequence, only if we're in the middle of the song
        if (musicPlayer.CurrentPlayTime > 0f) StartCoroutine(LevelUnpauseSequence());
    }

    public void QuitLevel()
    {
        Unload();
        // Back to menu screen
        switch (currentMode)
        {
            case Mode.ARCADE:
                UILevelSelection.unlockedLevelCount = gameState.GetUnlockedLevelCount();
                // Submit score
                if (gameState.IsArcadeHighscore(gameState.CurrentArcadeScore))
                    StartCoroutine(SubmitArcadeHighscore());
                else
                    UIMainMenu.ShowUI();
                break;
            case Mode.ONE_LEVEL:
                UILevelSelection.ShowUI();
                break;
        }
    }

    private IEnumerator LevelStartSequence()
    {
        // Wait for music to finish loading
        while (musicPlayer.IsLoading) yield return null;
        // Initialize note spawn: display first notes
        //noteSpawner.SpawnNotes(musicPlayer.loadedNotes, -noteSpawner.SpawnDelay, 0f);
        // Play metronome click
        Metronome metronome = musicPlayer.metronome;
        metronome.timeMode = Metronome.TimeMode.FixedUpdate;
        metronome.click = true;
        metronome.enabled = true;
        // Initialize countdown
        ClickTimer countdown = new ClickTimer(metronome);
        countdown.SetStepDuration(-1, minimumCountdownStepDuration);
        countdown.onStep.AddListener(OnCountdownStep);
        int countdownStartStep = (int)metronome.CurrentBar.durationInBeats;
        while (countdownStartStep < startCountdownValue) countdownStartStep *= 2;
        // Wait for player to grab the trombone, then start countdown
        GUI.ShowGrabTromboneMessage();
        bool countDownEnd = false;
        bool grabbedTrombone = false;
        while (countDownEnd == false || grabbedTrombone == false)
        {
            if (trombone.grab)
            {
                // Trombone is grabbed: start countdown
                if (grabbedTrombone == false)
                {
                    grabbedTrombone = true;
                    countdown.step = countdownStartStep;
                    countdown.StartOnNextBar();
                }
            }
            else
            {
                // Trombone is released :interrupt countdown
                if (grabbedTrombone == true)
                {
                    grabbedTrombone = false;
                    GUI.ShowGrabTromboneMessage();
                    countdown.Stop();
                }
            }
            // Check end of countdown
            countDownEnd = countdown.step <= 0f;
            yield return null;
        }
        // Countdown end
        countdown.Stop();
        countdown.onStep.RemoveListener(OnCountdownStep);
        // Silence metronome and sync it on music player
        metronome.timeMode = Metronome.TimeMode.FollowPlayhead;
        metronome.click = false;
        // Start music
        musicPlayer.Play();
        //musicPlayer.SetPlaytimeSamples(metronome.GetClickTimeSamples());
    }

    private void OnCountdownStep(int step)
    {
        // Show countdown
        if (step >= 0 && step <= startCountdownValue) GUI.ShowCountdown(step);
    }

    private IEnumerator LevelUnpauseSequence()
    {
        // Wait for player to grab the trombone
        GUI.ShowGrabTromboneMessage();
        yield return new WaitUntil(() => trombone.grab == true);
        // Play music
        GUI.ClearMessage();
        musicPlayer.Play();
    }

    private void OnLevelEnd()
    {
        // Unload level
        Unload();
        // Score
        if (gameState != null)
        {
            LevelScoreInfo scoreInfo = perfJudge.GetLevelScore();
            int levelIndex = gameState.currentLevelIndex;
            gameState.SetLevelScore(levelIndex, scoreInfo);
            // Show score screen
            UIScore.DisplayScore(levelIndex, gameState.CurrentLevel.Name, scoreInfo);
            if (gameState.IsLevelHighscore(scoreInfo.Total, levelIndex))
                UIScore.onFinishDisplayScore.AddListener(OnLevelHighscore);
            else
                UIScore.onFinishDisplayScore.AddListener(OnScoreDisplayEnd);
        }
    }

    private void OnScore(float score)
    {
        // Update GUI
        GUI.SetScore(score);
    }

    private void OnHealth(float health)
    {
        // Update GUI
        GUI.SetHealthBar(health);
        // Death
        if (health <= 0f)
        {
            DisablePerformanceJudge();
            musicPlayer.Stop(gameOverTransitionDuration);
            Invoke("GameOver", gameOverTransitionDuration);
        }
    }

    private void OnDance(int dance)
    {
        // Update GUI
        GUI.SetDanceBar(dance);
        // Enable power
        if (dance >= perfJudge.danceLength) GUI.SetPowerButton(true);
    }

    private void OnNotePerformanceCorrect(NoteInstance note, float accuracy, float points)
    {
        // Update GUI
        GUI.SetNoteAccuracy(accuracy);
        GUI.SetNotePoints(points, note.DisplayColor);
    }

    private void OnNotePerformanceWrong(NoteInstance note)
    {
        // Update GUI
        //GUI.SetNoteAccuracy(0f);
        GUI.ShowMissedMessage();
    }

    private void OnNotePerformanceMiss(NoteInstance note)
    {
        // Update GUI
        //GUI.ShowMissedMessage();
    }

    private void OnNotePerformanceEnd(NoteInstance note, float points, bool fullPlay)
    {
        // Update GUI
        if (note == null) GUI.EndNotePoints();
        else GUI.EndNotePoints(points, note.DisplayColor);
        GUI.SetNoteCombo(perfJudge.combo);
    }

    private void GameOver()
    {
        switch (currentMode)
        {
            case Mode.ARCADE:
                UIGameOver.DisplayGameOver(gameState.continues);
                UIGameOver.onContinue.AddListener(OnContinue);
                break;
            case Mode.ONE_LEVEL:
                UIGameOver.DisplayGameOver();
                UIGameOver.onContinue.AddListener(OnContinue);
                break;
        }
    }

    private void OnContinue(bool useContinue)
    {
        UIGameOver.onContinue.RemoveListener(OnContinue);
        if (useContinue)
        {
            if (currentMode == Mode.ARCADE) gameState.continues--;
            RestartLevel();
        }
        else
        {
            QuitLevel();
        }
    }

    private void OnScoreDisplayEnd()
    {
        UIScore.onFinishDisplayScore.RemoveListener(OnScoreDisplayEnd);
        if (currentMode == Mode.ARCADE)
        {
            // Unlock next level
            gameState.UnlockLevel(gameState.currentLevelIndex + 1);
            // Load next level
            LoadLevel(Mode.ARCADE, gameState.CurrentLevelNumber + 1);
        }
        else
            QuitLevel();
    }

    private void OnLevelHighscore()
    {
        UIScore.onFinishDisplayScore.RemoveListener(OnLevelHighscore);
        StartCoroutine(SubmitLevelHighscore());
    }

    private IEnumerator SubmitLevelHighscore()
    {
        LevelScoreInfo score = perfJudge.GetLevelScore();
        int levelIndex = gameState.currentLevelIndex;
        string playerName = null;
        UIHighScore.DisplayLevelHighscoreInput(gameState.CurrentLevel.Name, score.Total);
        UIHighScore.onSubmitName.AddListener(s => playerName = s);
        yield return new WaitWhile(() => playerName == null);
        UIHighScore.onSubmitName.RemoveAllListeners();
        gameState.SetLevelHighscore(score.Total, levelIndex, playerName);
        OnScoreDisplayEnd();
    }

    private IEnumerator SubmitArcadeHighscore()
    {
        int score = gameState.CurrentArcadeScore;
        int rank = gameState.GetArcadeScoreRank(score);
        string playerName = null;
        UIHighScore.DisplayArcadeHighscoreInput(score, rank);
        UIHighScore.onSubmitName.AddListener(s => playerName = s);
        yield return new WaitWhile(() => playerName == null);
        UIHighScore.onSubmitName.RemoveAllListeners();
        gameState.SetArcadeHighscore(score, rank, playerName);
        UIMainMenu.ShowUI();
    }

    private void UpdateLevelSelection()
    {
        // Update level selection screen
        UILevelSelection.unlockedLevelCount = gameState.GetUnlockedLevelCount();
        UILevelSelection.levelNames = gameState.LevelNames;
        UILevelSelection.UpdateLevelList();
    }

    private void UpdateLeaderBoard()
    {
        // Update leaderboard screen
        UILeaderboard.unlockedLevelCount = gameState.GetUnlockedLevelCount();
        UILeaderboard.levelNames = gameState.LevelNames;
        UILeaderboard.levelHighScores = gameState.LevelHighScores;
        UILeaderboard.arcadeHighScores = gameState.ArcadeHighScores;
    }

    private void CheatKeys()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.X))
                perfJudge.TakeDamage(1f);
            else if (Input.GetKeyDown(KeyCode.H))
                perfJudge.HealDamage(1f);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                musicPlayer.playingSpeed -= .1f;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                musicPlayer.playingSpeed += .1f;
            else if (Input.GetKeyDown(KeyCode.F))
                OnLevelEnd();
            else if (Input.GetKey(KeyCode.P))
            {
                if (musicPlayer.Playing == MusicPlayer.PlayState.Play)
                    musicPlayer.Pause();
                else if (musicPlayer.Playing == MusicPlayer.PlayState.Pause)
                    musicPlayer.Play();
            }
        }
    }
}
