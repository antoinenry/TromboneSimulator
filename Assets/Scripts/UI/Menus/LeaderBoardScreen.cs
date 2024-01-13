using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LeaderBoardScreen : MenuUI
{
    [Header("Navigation")]
    public Button previousPageButton;
    public Button nextPageButton;
    public Button backButton;
    [Header("Arcade Page")]
    public RectTransform arcadePage;
    public ScoreLineDisplay[] arcadeLineDisplays;
    [Header("Levels Page")]
    public RectTransform levelPage;
    public ScoreLineDisplay[] levelLineDisplays;
    [Header("Contents")]
    public int currentPage;
    public HighScoreInfos[] arcadeHighScores;
    public HighScoreInfos[] levelHighScores;
    public string[] levelNames;
    public int unlockedLevelCount = 3;
    [Header("Events")]
    public UnityEvent onGoBack;

    private int totalPageCount;
    private int levelLineCount;
    private int circlePage;

    protected override void Awake()
    {
        base.Awake();
        UILeaderboard = this;
    }

    public override void ShowUI()
    {
        base.ShowUI();
        if (Application.isPlaying)
        {
            nextPageButton.onClick.AddListener(CircleNextPage);
            previousPageButton.onClick.AddListener(CirclePreviousPage);
            backButton.onClick.AddListener(GoBack);
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        if (Application.isPlaying)
        {
            nextPageButton.onClick.RemoveListener(CircleNextPage);
            previousPageButton.onClick.RemoveListener(CirclePreviousPage);
            backButton.onClick.RemoveListener(GoBack);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (IsVisible)
        {
            // Count pages depending on unlocked levels and number of lines per page
            totalPageCount = 2;
            levelLineCount = levelLineDisplays != null ? levelLineDisplays.Length : 0;
            totalPageCount += (unlockedLevelCount - 1) / levelLineCount;
            circlePage = (int)Mathf.Repeat(currentPage, totalPageCount);

            if (circlePage == 0)
            {
                // Hide level page
                if (levelPage != null)
                {
                    levelPage.gameObject.SetActive(false);
                }
                // Show arcade page
                if (arcadePage != null)
                {
                    arcadePage.gameObject.SetActive(true);
                    ArcadePageUpdate();
                }
            }
            else
            {
                // Hide arcade page
                if (arcadePage != null)
                {
                    arcadePage.gameObject.SetActive(false);
                }
                // Show level page
                if (levelPage != null)
                {
                    levelPage.gameObject.SetActive(true);
                    LevelPageUpdate();
                }
            }
        }
    }

    private void ArcadePageUpdate()
    {
        int lineCount = arcadeLineDisplays != null ? arcadeLineDisplays.Length : 0;
        for (int i = 0; i < lineCount; i++)
        {
            ScoreLineDisplay line = arcadeLineDisplays[i];
            if (line == null) continue;
            HighScoreInfos highscore = new HighScoreInfos();
            if (i < arcadeHighScores.Length) highscore = arcadeHighScores[i];
            if (highscore.playerName == "" || highscore.playerName == null)
                line.SetTextAt(0, "AAA");
            else
                line.SetTextAt(0, highscore.playerName);
            line.SetValueAt(0, highscore.score);
        }
    }

    private void LevelPageUpdate()
    {        
        int lineCount = levelLineDisplays != null ? levelLineDisplays.Length : 0;
        int highscoreCount = levelHighScores.Length;
        int levelCount = levelNames.Length;
        for (int i = 0; i < lineCount; i++)
        {
            ScoreLineDisplay line = levelLineDisplays[i];
            if (line == null) continue;
            int highscoreIndex = (circlePage - 1) * lineCount + i;
            if (highscoreIndex < highscoreCount && highscoreIndex < levelCount && highscoreIndex < unlockedLevelCount)
            {
                line.visible = true;
                HighScoreInfos highscore = levelHighScores[highscoreIndex];
                line.SetTextAt(0, levelNames[highscoreIndex]);
                if (highscore.playerName == "" || highscore.playerName == null)
                    line.SetTextAt(1, "AAA");
                else
                    line.SetTextAt(1, highscore.playerName);
                line.SetValueAt(0, highscore.score);
            }
            else
            {
                line.visible = false;
            }
        }
    }

    private void CircleNextPage()
    {
        currentPage = (int)Mathf.Repeat(currentPage + 1, totalPageCount);
    }

    private void CirclePreviousPage()
    {
        currentPage = (int)Mathf.Repeat(currentPage -1, totalPageCount);
    }

    //private void GoBack()
    //{
    //    onGoBack.Invoke();
    //    onGoBack.RemoveAllListeners();
    //    HideUI();
    //}
}
