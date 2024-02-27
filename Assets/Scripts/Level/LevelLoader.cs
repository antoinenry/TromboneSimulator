using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public enum Mode { ARCADE, ONE_LEVEL }

    public bool cheatMode = false;
    [Header("Transitions")]
    public float minimumCountdownStepDuration = 1.5f;
    public int startCountdownValue = 3;
    public float gameOverTransitionDuration = 2f;
    public float levelEndTransitionDuration = 2f;
    [Header("TO MOVE")]
    //public GameState_old gameState;

    // Coroutines
    private Coroutine loadLevelCoroutine;
    private Coroutine startLevelCoroutine;
    private Coroutine unpauseLevelCoroutine;
    // Component references
    private Trombone trombone;
    private MusicPlayer musicPlayer;
    private NoteCatcher noteCatcher;
    private NoteSpawner noteSpawner;
    private PerformanceJudge perfJudge;
    private LevelGUI GUI;
    // Load state
    public Mode LoadedMode { get; private set; }
    public Level LoadedLevel { get; private set; }
    public float MusicProgress => musicPlayer ? musicPlayer.CurrentPlayTime / musicPlayer.MusicDuration : 0f;

    #region INIT
    private void Awake()
    {
        // Find components
        trombone = FindObjectOfType<Trombone>(true);
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        noteCatcher = FindObjectOfType<NoteCatcher>(true);
        noteSpawner = FindObjectOfType<NoteSpawner>(true);
        perfJudge = FindObjectOfType<PerformanceJudge>(true);
        GUI = FindObjectOfType<LevelGUI>(true);
        // Clear level load
        UnloadLevel();
    }

    private void OnEnable()
    {
        if (MenuUI.UILevelSelection) MenuUI.UILevelSelection.onSelectLevel.AddListener(LaunchOneLevelMode);
    }

    private void OnDisable()
    {
        if (MenuUI.UILevelSelection) MenuUI.UILevelSelection.onSelectLevel.RemoveListener(LaunchOneLevelMode);
    }
    #endregion

    #region LAUNCH
    public void LaunchArcadeMode()
    {
        //gameState.continues = startContinues;
        //gameState.ClearCurrentScore();
        //LoadLevel(Mode.ARCADE, 1);
    }

    public void LaunchOneLevelMode(Level level)
    {
        //gameState.continues = 0;
        //gameState.ClearCurrentScore();
        LoadLevel(Mode.ONE_LEVEL, level);
        StartLevel();
    }

    #endregion

    #region LOAD/UNLOAD
    public void LoadLevel(Mode mode, Level level)
    {
        LoadedMode = mode;
        LoadedLevel = level;
        loadLevelCoroutine = StartCoroutine(LoadLevelCoroutine());
    }

    private IEnumerator LoadLevelCoroutine()
    {
        // Music setup
        if (musicPlayer)
        {
            musicPlayer.Stop();
            musicPlayer.LoadMusic(LoadedLevel.music, playedInstrument: trombone.Sampler);            
            musicPlayer.loop = false;
            musicPlayer.onPlayerUpdate.AddListener(OnMusicPlayerUpdate);
        }
        // Trombone setup
        if (trombone)
        {
            trombone.LoadBuild();
            trombone.ResetTrombone();
            trombone.Unfreeze();
        }
        // NoteSpawner setup
        if (noteSpawner) noteSpawner.enabled = true;
        // Note catcher setup
        if (noteCatcher)
        {
            noteCatcher.enabled = true;
            noteCatcher.trombone = trombone;
        }
        // Judge setup
        if (perfJudge)
        {
            perfJudge.enabled = true;
            if (trombone.Sampler != null) perfJudge.LevelSetup(LoadedLevel.music, trombone.Sampler);
            perfJudge.onHealth.AddListener(OnHealthChange);
        }
        // GUI Setup
        if (GUI)
        {
            if (musicPlayer) yield return new WaitWhile(() => musicPlayer.IsLoading);
            GUI.GUIActive = true;
            GUI.onPauseRequest.AddListener(PauseLevel);
            GUI.SetPauseButtonActive(true);
        }
        loadLevelCoroutine = null;
    }

    public void UnloadLevel()
    {
        LoadedLevel = null;
        // Stop GUI
        if (GUI)
        {
            GUI.GUIActive = false;
            GUI.SetPauseButtonActive(false);
            GUI.onPauseRequest.RemoveListener(PauseLevel);
        }
        // Stop music
        if (musicPlayer)
        {
            musicPlayer.Stop();
            musicPlayer.UnloadMusic();
            musicPlayer.onPlayerUpdate.RemoveListener(OnMusicPlayerUpdate);
        }
        // Stop performance judge
        if (perfJudge)
        {
            perfJudge.onHealth.RemoveListener(OnHealthChange);
            perfJudge.enabled = false;
        }
        // Stop note spawner
        if (noteSpawner) noteSpawner.enabled = false;
        // Stop note catcher
        if (noteCatcher) noteCatcher.enabled = false;
    }
    #endregion

    #region START
    public void StartLevel()
    {
        // Level start sequence
        if (startLevelCoroutine != null) StopCoroutine(LevelStartCoroutine());
        startLevelCoroutine = StartCoroutine(LevelStartCoroutine());
    }
    private IEnumerator LevelStartCoroutine()
    {
        // Wait for music to finish loading
        while (musicPlayer.IsLoading) yield return null;
        // Initialize note spawn: display first notes
        noteSpawner.SpawnNotes(musicPlayer.LoadedNotes, -noteSpawner.SpawnDelay, 0f);
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

    private void OnMusicPlayerUpdate()
    {
        if (musicPlayer == null) return;
        if (musicPlayer.CurrentPlayTime >= musicPlayer.MusicDuration) OnLevelEnd();
        if (GUI) GUI.SetTimeBar(musicPlayer.LoopedPlayTime, musicPlayer.MusicDuration);
    }
    #endregion

    #region PAUSE/UNPAUSE
    public void PauseLevel()
    {
        // Show pause screen
        if (MenuUI.UIPause)
        {
            MenuUI.UIPause.ShowUI();
            MenuUI.UIPause.onUnpause.AddListener(UnpauseLevel);
            MenuUI.UIPause.onQuit.AddListener(QuitLevel);
        }
        // Toggle pause button behaviour
        if (GUI)
        {
            GUI.onPauseRequest.RemoveListener(PauseLevel);
            GUI.onPauseRequest.AddListener(UnpauseLevel);
        }
        // Pause game and music
        if (musicPlayer) musicPlayer.Pause(true);
        if (trombone) trombone.Freeze();
        // Interrupt unpause sequence (grabbing trombone)
        if (unpauseLevelCoroutine != null) StopCoroutine(unpauseLevelCoroutine);
    }

    public void UnpauseLevel()
    {
        // Hide pause screen
        if (MenuUI.UIPause)
        {
            MenuUI.UIPause.HideUI();
            MenuUI.UIPause.onUnpause.RemoveListener(UnpauseLevel);
            MenuUI.UIPause.onQuit.RemoveListener(QuitLevel);
        }
        // Toggle pause button behaviour
        if (GUI)
        {
            GUI.onPauseRequest.RemoveListener(UnpauseLevel);
            GUI.onPauseRequest.AddListener(PauseLevel);
        }
        // Start unpause sequence (wait for trombone to be grabbed), unless the game was paused before the song starts (then it's back to countdown sequence)
        if (trombone) trombone.Unfreeze();
        if (musicPlayer && musicPlayer.CurrentPlayTime > 0f) unpauseLevelCoroutine = StartCoroutine(LevelUnpauseCoroutine());
    }

    private IEnumerator LevelUnpauseCoroutine()
    {
        // Wait for player to grab the trombone
        GUI.ShowGrabTromboneMessage();
        yield return new WaitUntil(() => trombone.grab == true);
        // Play music
        GUI.ClearMessage();
        musicPlayer.Play();
    }
    #endregion

    #region GAMEOVER/CONTINUE
    private void OnHealthChange(float healthValue, float healthDelta)
    {
        if (healthValue <= 0f) StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        // Transition
        trombone.Freeze();
        perfJudge.DisableDetection();
        GUI.SetPauseButtonActive(false);
        musicPlayer.Stop(gameOverTransitionDuration);
        // Wait
        yield return new WaitForSeconds(gameOverTransitionDuration); 
        // End of transition
        trombone.Unfreeze();
        // Display a different screen depending on game mode
        switch (LoadedMode)
        {
            case Mode.ARCADE:
                //MenuUI.UIGameOver.DisplayGameOver(gameState.continues);
                //MenuUI.UIGameOver.onContinue.AddListener(OnContinue);
                break;
            case Mode.ONE_LEVEL:
                if (MenuUI.UIGameOver)
                {
                    MenuUI.UIGameOver.DisplayGameOver();
                    MenuUI.UIGameOver.onContinue.AddListener(OnContinue);
                }
                break;
        }
    }

    private void OnContinue(bool useContinue)
    {
        MenuUI.UIGameOver.onContinue.RemoveListener(OnContinue);
        if (useContinue)
        {
            //if (currentMode == Mode.ARCADE) gameState.continues--;
            trombone.ResetTrombone();
            perfJudge.ResetPerformance();
            perfJudge.EnableDetection();
            StartLevel();
        }
        else
        {
            QuitLevel();
        }
    }
    #endregion

    #region END/QUIT
    private void OnLevelEnd()
    {
        StartCoroutine(LevelEndCoroutine());
    }

    private IEnumerator LevelEndCoroutine()
    {
        // Stop level
        GUI.SetPauseButtonActive(false);
        musicPlayer.Stop();
        // Transition
        yield return new WaitForSeconds(levelEndTransitionDuration);
        // Unload level
        UnloadLevel();
        LevelScoreInfo scoreInfo = perfJudge.GetLevelScore();
        if (MenuUI.UIScore)
        {
            // Show score screen
            if (LoadedLevel) MenuUI.UIScore.DisplayScore(LoadedLevel.name, scoreInfo);
            else MenuUI.UIScore.DisplayScore("", scoreInfo);
            yield return new WaitWhile(() => MenuUI.UIScore.IsVisible);
        }
        QuitLevel();
    }

    public void QuitLevel()
    {
        UnloadLevel();
        // Back to menu screen
        switch (LoadedMode)
        {
            case Mode.ARCADE:
                //MenuUI.UILevelSelection.unlockedLevelCount = gameState.GetUnlockedLevelCount();
                //// Submit score
                //if (gameState.IsArcadeHighscore(gameState.CurrentArcadeScore))
                //    submitArcadeHighscoreCoroutine = StartCoroutine(SubmitArcadeHighscore());
                //else
                //    MenuUI.UIMainMenu.ShowUI();
                //break;
            case Mode.ONE_LEVEL:
                MenuUI.UILevelSelection.ShowUI();
                break;
        }
    }
    #endregion

    #region DEPRECIATED
    //private void CheatKeys()
    //{
    //    if (Input.anyKeyDown)
    //    {
    //        if (Input.GetKeyDown(KeyCode.X))
    //            perfJudge.TakeDamage(1f);
    //        else if (Input.GetKeyDown(KeyCode.H))
    //            perfJudge.HealDamage(1f);
    //        else if (Input.GetKeyDown(KeyCode.LeftArrow))
    //            musicPlayer.playingSpeed -= .1f;
    //        else if (Input.GetKeyDown(KeyCode.RightArrow))
    //            musicPlayer.playingSpeed += .1f;
    //        else if (Input.GetKeyDown(KeyCode.F))
    //            OnLevelEnd();
    //        else if (Input.GetKey(KeyCode.P))
    //        {
    //            if (musicPlayer.State == MusicPlayer.PlayState.Play)
    //                musicPlayer.Pause();
    //            else if (musicPlayer.State == MusicPlayer.PlayState.Pause)
    //                musicPlayer.Play();
    //        }
    //    }
    //}    

    //private void OnLevelHighscore()
    //{
    //    MenuUI.UIScore.onFinishDisplayScore.RemoveListener(OnLevelHighscore);
    //    submitLevelHighscoreCoroutine = StartCoroutine(SubmitLevelHighscore());
    //}

    //private IEnumerator SubmitLevelHighscore()
    //{
    //    LevelScoreInfo score = perfJudge.GetLevelScore();
    //    int levelIndex = gameState.currentLevelIndex;
    //    string playerName = null;
    //    MenuUI.UIHighScore.DisplayLevelHighscoreInput(gameState.CurrentLevel.Name, score.Total);
    //    MenuUI.UIHighScore.onSubmitName.AddListener(s => playerName = s);
    //    yield return new WaitWhile(() => playerName == null);
    //    MenuUI.UIHighScore.onSubmitName.RemoveAllListeners();
    //    gameState.SetLevelHighscore(score.Total, levelIndex, playerName);
    //    OnScoreDisplayEnd();
    //}

    //private IEnumerator SubmitArcadeHighscore()
    //{
    //    int score = gameState.CurrentArcadeScore;
    //    int rank = gameState.GetArcadeScoreRank(score);
    //    string playerName = null;
    //    MenuUI.UIHighScore.DisplayArcadeHighscoreInput(score, rank);
    //    MenuUI.UIHighScore.onSubmitName.AddListener(s => playerName = s);
    //    yield return new WaitWhile(() => playerName == null);
    //    MenuUI.UIHighScore.onSubmitName.RemoveAllListeners();
    //    gameState.SetArcadeHighscore(score, rank, playerName);
    //    MenuUI.UIMainMenu.ShowUI();
    //}

    //private void UpdateLevelSelection()
    //{
    //    // Update level selection screen
    //    //MenuUI.UILevelSelection.unlockedLevelCount = gameState.GetUnlockedLevelCount();
    //    //MenuUI.UILevelSelection.levelNames = gameState.LevelNames;
    //    //MenuUI.UILevelSelection.UpdateLevelList();
    //}

    //private void UpdateLeaderBoard()
    //{
        // Update leaderboard screen
        //MenuUI.UILeaderboard.unlockedLevelCount = gameState.GetUnlockedLevelCount();
        //MenuUI.UILeaderboard.levelNames = gameState.LevelNames;
        //MenuUI.UILeaderboard.levelHighScores = gameState.LevelHighScores;
        //MenuUI.UILeaderboard.arcadeHighScores = gameState.ArcadeHighScores;
    //}
    #endregion
}
