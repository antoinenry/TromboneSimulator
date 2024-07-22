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
    private TromboneCore trombone;
    private MusicPlayer musicPlayer;
    private NoteCatcher noteCatcher;
    private NoteSpawner noteSpawner;
    private PerformanceJudge performanceJudge;
    private ObjectiveJudge objectiveJudge;
    private LevelGUI GUI;

    // Load state
    public Mode LoadedMode { get; private set; }
    public Level LoadedLevel { get; private set; }

    #region INIT
    private void Awake()
    {
        // Find components
        trombone = FindObjectOfType<TromboneCore>(true);
        musicPlayer = FindObjectOfType<MusicPlayer>(true);
        noteCatcher = FindObjectOfType<NoteCatcher>(true);
        noteSpawner = FindObjectOfType<NoteSpawner>(true);
        performanceJudge = FindObjectOfType<PerformanceJudge>(true);
        objectiveJudge = FindObjectOfType<ObjectiveJudge>(true);
        GUI = FindObjectOfType<LevelGUI>(true);
        // Clear level load
        UnloadLevel();
    }

    private void OnEnable()
    {
        MenuUI.onSelectLevel.AddListener(OnMenuSelectLevel);
        MenuUI.onStartLevel.AddListener(OnMenuStartLevel);
    }

    private void OnDisable()
    {
        MenuUI.onSelectLevel.RemoveListener(OnMenuSelectLevel);
        MenuUI.onStartLevel.RemoveListener(OnMenuStartLevel);
    }
    #endregion

    #region MENU CONTROLS
    private void OnMenuSelectLevel(Level selectedLevel)
    {

    }
        
    private void OnMenuStartLevel()
    {
        Level selectedLevel = MenuUI.Get<LevelSelectionScreen>()?.GetSelectedLevel();
        LoadLevel(Mode.ONE_LEVEL, selectedLevel);
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
        // Trombone setup
        if (trombone)
        {
            trombone.ResetTrombone();
            trombone.Unfreeze();
        }
        // Music setup
        if (musicPlayer)
        {
            musicPlayer.Stop();
            musicPlayer.loop = false;
            musicPlayer.LoadMusic(LoadedLevel.music, playedInstrument: trombone.Sampler);
            //musicPlayer.onPlayerUpdate.AddListener(OnMusicPlayerUpdate);
            musicPlayer.onMusicEnd.AddListener(OnMusicEnd);
        }
        // NoteSpawner setup
        if (noteSpawner)
        {
            noteSpawner.enabled = true;
            noteSpawner.notePlacement = LoadedLevel.GetNoteCoordinates(noteSpawner.GridDimensions);
        }
        // Note catcher setup
        if (noteCatcher)
        {
            noteCatcher.enabled = true;
            noteCatcher.trombone = trombone;
        }
        // Judges setup
        if (performanceJudge)
        {
            performanceJudge.enabled = true;
            performanceJudge.Initialize(LoadedLevel?.music, trombone);
            performanceJudge.onHealth.AddListener(OnHealthChange);
        }
        if (objectiveJudge)
        {
            objectiveJudge.enabled = true;
            objectiveJudge.LoadObjectives(LoadedLevel.objectives);
        }
        // GUI Setup
        if (GUI)
        {
            if (musicPlayer) yield return new WaitWhile(() => musicPlayer.IsLoading);
            GUI.GUIActive = true;
            GUI.SetPauseButtonActive(true);
            GUI.onPauseRequest.AddListener(PauseLevel);
        }
        loadLevelCoroutine = null;
    }

    public void UnloadLevel()
    {
        // Stop GUI
        if (GUI)
        {
            GUI.GUIActive = false;
            GUI.SetPauseButtonActive(false);
            GUI.onPauseRequest.RemoveListener(PauseLevel);
        }
        // Stop level music
        if (musicPlayer)
        {
            if (musicPlayer.IsLoadedMusic(LoadedLevel?.music))
            {
                musicPlayer.Stop();
                musicPlayer.UnloadMusic();
            }
            //musicPlayer.onPlayerUpdate.RemoveListener(OnMusicPlayerUpdate);
            musicPlayer.onMusicEnd.RemoveListener(OnMusicEnd);
        }
        // Stop judging
        if (performanceJudge)
        {
            performanceJudge.onHealth.RemoveListener(OnHealthChange);
            performanceJudge.enabled = false;
        }
        if (objectiveJudge)
        {
            objectiveJudge.UnloadObjectives();
            objectiveJudge.enabled = false;
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
        // Reactivate pause button
        GUI?.SetPauseButtonActive(true);
        // Initialize note spawn: display first notes
        noteSpawner.time = 0f;
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
        //if (musicPlayer.CurrentPlayTime >= musicPlayer.MusicDuration) OnLevelEnd();
        if (GUI) GUI.SetTimeBar(musicPlayer.LoopedPlayTime, musicPlayer.MusicDuration);
    }

    private void OnMusicEnd()
    {
        StartCoroutine(LevelEndCoroutine());
    }

    #endregion

    #region PAUSE/UNPAUSE
    public void PauseLevel()
    {
        // Show pause screen
        PauseScreen UIPause = MenuUI.Get<PauseScreen>();
        if (UIPause)
        {
            UIPause.ShowUI();
            UIPause.onUnpause.AddListener(UnpauseLevel);
            UIPause.onQuit.AddListener(QuitLevel);
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
        PauseScreen UIPause = MenuUI.Get<PauseScreen>();
        if (UIPause)
        {
            UIPause.HideUI();
            UIPause.onUnpause.RemoveListener(UnpauseLevel);
            UIPause.onQuit.RemoveListener(QuitLevel);
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
    private void OnHealthChange(float healthValue, float maxHealth, float healthDelta)
    {
        if (healthValue <= 0f) StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        // Transition
        trombone.Freeze();
        performanceJudge.DisableDetection();
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
                GameOverScreen UIGameOver = MenuUI.Get<GameOverScreen>();
                if (UIGameOver)
                {
                    UIGameOver.DisplayGameOver();
                    UIGameOver.onContinue.AddListener(OnContinue);
                }
                break;
        }
    }

    private void OnContinue(bool useContinue)
    {
        MenuUI.Get<GameOverScreen>()?.onContinue?.RemoveListener(OnContinue);
        if (useContinue)
        {
            //if (currentMode == Mode.ARCADE) gameState.continues--;
            trombone.ResetTrombone();
            performanceJudge.ResetPerformance();
            performanceJudge.EnableDetection();
            StartLevel();
        }
        else
        {
            QuitLevel();
        }
    }
    #endregion

    #region END/QUIT
    private IEnumerator LevelEndCoroutine()
    {
        // Wait one frame to ensure other method called on LevelEnd are executed beforehand
        yield return null;
        // Stop level
        GUI.SetPauseButtonActive(false);
        musicPlayer.Stop();
        // Get level performance
        LevelScoreInfo levelScore = performanceJudge.GetLevelScore();
        ObjectiveInfo[] completedObjectives = objectiveJudge.GetCompletedObjectives();
        LevelProgress levelProgress = new LevelProgress(LoadedLevel, completedObjectives);
        // Update general progress
        GameProgress progress = GameProgress.Current;
        if (progress) progress.CompleteObjectives(LoadedLevel, completedObjectives);
        // Transition
        yield return new WaitForSeconds(levelEndTransitionDuration);
        // Unload level
        UnloadLevel();
        // Show score screen
        LevelCompleteScreen UILevelComplete = MenuUI.Get<LevelCompleteScreen>();
        if (UILevelComplete)
        {
            UILevelComplete.levelProgress = levelProgress;
            UILevelComplete.levelScore = levelScore;
            UILevelComplete.ShowUI();
            yield return new WaitWhile(() => UILevelComplete.IsVisible);
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
                MenuUI.Get<LevelSelectionScreen>()?.ShowUI();
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
