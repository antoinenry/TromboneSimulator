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

    // Coroutines
    private Coroutine startLevelCoroutine;
    private Coroutine unpauseLevelCoroutine;
    private Coroutine submitLevelHighscoreCoroutine;
    private Coroutine submitArcadeHighscoreCoroutine;
    // Component references
    private MusicPlayer musicPlayer;
    private NoteCatcher noteCatcher;
    private NoteSpawner noteSpawner;
    private PerformanceJudge perfJudge;
    private Trombone trombone;
    private LevelGUI GUI;

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

    private void Start()
    {
        // Permanents listeners (even when LevelLoader is disabled)
        gameState.onChangeGameSettings.AddListener(ApplySettings);
        MenuUI.UIMainMenu.onLaunchArcade.AddListener(LaunchArcadeMode);
        MenuUI.UILevelSelection.onShowUI.AddListener(UpdateLevelSelection);
        MenuUI.UILevelSelection.onSelectLevel.AddListener(LaunchOneLevelMode);
        MenuUI.UILeaderboard.onShowUI.AddListener(UpdateLeaderBoard);
    }

    private void OnDestroy()
    {
        // Remove permanents listeners
        gameState.onChangeGameSettings.RemoveListener(ApplySettings);
        MenuUI.UIMainMenu.onLaunchArcade.RemoveListener(LaunchArcadeMode);
        MenuUI.UILevelSelection.onShowUI.RemoveListener(UpdateLevelSelection);
        MenuUI.UILevelSelection.onSelectLevel.RemoveListener(LaunchOneLevelMode);
        MenuUI.UILeaderboard.onShowUI.RemoveListener(UpdateLeaderBoard);
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
        trombone.LoadBuild();
        trombone.ResetTrombone();
        trombone.Unfreeze();
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
        GUI.onPressPause.AddListener(PauseLevel);
        GUI.SetPauseButtonActive(true);
        // Level start sequence
        startLevelCoroutine = StartCoroutine(LevelStartSequence());
    }

    public void Unload()
    {
        LoadedLevel = null;
        LoadedLevelAudio = null;
        // Stop GUI
        GUI.GUIActive = false;
        GUI.SetPauseButtonActive(false);
        GUI.onPressPause.RemoveListener(PauseLevel);
        // Stop music
        musicPlayer.Stop();
        musicPlayer.UnloadMusic();
        musicPlayer.onMusicEnd.RemoveListener(OnLevelEnd);
        // Stop performance judge
        DisablePerformanceJudge();
        // Stop note spawner
        noteSpawner.enabled = false;
        // Stop note catcher
        noteCatcher.enabled = false;
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
        // Show pause screen
        MenuUI.UIPause.ShowUI();
        MenuUI.UIPause.onUnpause.AddListener(UnpauseLevel);
        MenuUI.UIPause.onQuit.AddListener(QuitLevel);
        // Toggle pause button behaviour
        GUI.onPressPause.RemoveListener(PauseLevel);
        GUI.onPressPause.AddListener(UnpauseLevel);
        // Pause game and music
        musicPlayer.Pause(true);
        trombone.Freeze();
        // Interrupt unpause sequence (grabbing trombone)
        if (unpauseLevelCoroutine != null) StopCoroutine(unpauseLevelCoroutine);
    }

    public void UnpauseLevel()
    {
        // Hide pause screen
        MenuUI.UIPause.HideUI();
        MenuUI.UIPause.onUnpause.RemoveListener(UnpauseLevel);
        MenuUI.UIPause.onQuit.RemoveListener(QuitLevel);
        // Toggle pause button behaviour
        GUI.onPressPause.RemoveListener(UnpauseLevel);
        GUI.onPressPause.AddListener(PauseLevel);
        // Start unpause sequence (wait for trombone to be grabbed), unless the game was paused before the song starts (then it's back to countdown sequence)
        trombone.Unfreeze();
        if (musicPlayer.CurrentPlayTime > 0f) unpauseLevelCoroutine = StartCoroutine(LevelUnpauseSequence());
    }

    public void QuitLevel()
    {
        Unload();
        // Back to menu screen
        switch (currentMode)
        {
            case Mode.ARCADE:
                MenuUI.UILevelSelection.unlockedLevelCount = gameState.GetUnlockedLevelCount();
                // Submit score
                if (gameState.IsArcadeHighscore(gameState.CurrentArcadeScore))
                    submitArcadeHighscoreCoroutine = StartCoroutine(SubmitArcadeHighscore());
                else
                    MenuUI.UIMainMenu.ShowUI();
                break;
            case Mode.ONE_LEVEL:
                MenuUI.UILevelSelection.ShowUI();
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
        // Disable pause button
        GUI.SetPauseButtonActive(false);
        // Unload level
        Unload();
        // Score
        if (gameState != null)
        {
            LevelScoreInfo scoreInfo = perfJudge.GetLevelScore();
            int levelIndex = gameState.currentLevelIndex;
            gameState.SetLevelScore(levelIndex, scoreInfo);
            // Show score screen
            MenuUI.UIScore.DisplayScore(levelIndex, gameState.CurrentLevel.Name, scoreInfo);
            if (gameState.IsLevelHighscore(scoreInfo.Total, levelIndex))
                MenuUI.UIScore.onFinishDisplayScore.AddListener(OnLevelHighscore);
            else
                MenuUI.UIScore.onFinishDisplayScore.AddListener(OnScoreDisplayEnd);
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
            // Stop level
            trombone.Freeze();
            DisablePerformanceJudge();
            musicPlayer.Stop(gameOverTransitionDuration);
            GUI.SetPauseButtonActive(false);
            // Start game over sequence
            Invoke("GameOver", gameOverTransitionDuration);
        }
    }

    private void OnDance(int dance)
    {
        // Update GUI
        GUI.SetDanceBar(dance);
        // Enable power
        if (dance >= perfJudge.danceLength) GUI.SetPowerButtonActive(true);
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
        // Trombone was frozen during transition and must be returned to normal
        trombone.Unfreeze();
        // Display a different screen depending on game mode
        switch (currentMode)
        {
            case Mode.ARCADE:
                MenuUI.UIGameOver.DisplayGameOver(gameState.continues);
                MenuUI.UIGameOver.onContinue.AddListener(OnContinue);
                break;
            case Mode.ONE_LEVEL:
                MenuUI.UIGameOver.DisplayGameOver();
                MenuUI.UIGameOver.onContinue.AddListener(OnContinue);
                break;
        }
    }

    private void OnContinue(bool useContinue)
    {
        MenuUI.UIGameOver.onContinue.RemoveListener(OnContinue);
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
        MenuUI.UIScore.onFinishDisplayScore.RemoveListener(OnScoreDisplayEnd);
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
        MenuUI.UIScore.onFinishDisplayScore.RemoveListener(OnLevelHighscore);
        submitLevelHighscoreCoroutine = StartCoroutine(SubmitLevelHighscore());
    }

    private IEnumerator SubmitLevelHighscore()
    {
        LevelScoreInfo score = perfJudge.GetLevelScore();
        int levelIndex = gameState.currentLevelIndex;
        string playerName = null;
        MenuUI.UIHighScore.DisplayLevelHighscoreInput(gameState.CurrentLevel.Name, score.Total);
        MenuUI.UIHighScore.onSubmitName.AddListener(s => playerName = s);
        yield return new WaitWhile(() => playerName == null);
        MenuUI.UIHighScore.onSubmitName.RemoveAllListeners();
        gameState.SetLevelHighscore(score.Total, levelIndex, playerName);
        OnScoreDisplayEnd();
    }

    private IEnumerator SubmitArcadeHighscore()
    {
        int score = gameState.CurrentArcadeScore;
        int rank = gameState.GetArcadeScoreRank(score);
        string playerName = null;
        MenuUI.UIHighScore.DisplayArcadeHighscoreInput(score, rank);
        MenuUI.UIHighScore.onSubmitName.AddListener(s => playerName = s);
        yield return new WaitWhile(() => playerName == null);
        MenuUI.UIHighScore.onSubmitName.RemoveAllListeners();
        gameState.SetArcadeHighscore(score, rank, playerName);
        MenuUI.UIMainMenu.ShowUI();
    }

    private void UpdateLevelSelection()
    {
        // Update level selection screen
        MenuUI.UILevelSelection.unlockedLevelCount = gameState.GetUnlockedLevelCount();
        MenuUI.UILevelSelection.levelNames = gameState.LevelNames;
        MenuUI.UILevelSelection.UpdateLevelList();
    }

    private void UpdateLeaderBoard()
    {
        // Update leaderboard screen
        MenuUI.UILeaderboard.unlockedLevelCount = gameState.GetUnlockedLevelCount();
        MenuUI.UILeaderboard.levelNames = gameState.LevelNames;
        MenuUI.UILeaderboard.levelHighScores = gameState.LevelHighScores;
        MenuUI.UILeaderboard.arcadeHighScores = gameState.ArcadeHighScores;
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
