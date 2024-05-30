using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class NewHighscoreScreen : MenuUI
{
    //public GameState gameState;
    [Header("UI Components")]
    public TextMeshProUGUI messageDisplay;
    public TextMeshProUGUI subMessageDisplay;
    public CounterDisplay scoreDisplay;
    public TMP_InputField inputField;
    [Header("Messages")]
    public string levelMessageText = "nouveau record !";
    public string arcadeBestMessageText = "nouveau record !";
    public string arcadeBestSubMessageText = "meilleur set de tous les temps";
    public string arcadeMessageText = "clap clap clap";
    public string arcadeSubMessageText = "le public est pendu a ton genie";

    public UnityEvent<string> onSubmitName;

    protected override void Update()
    {
        base.Update();
        if (IsVisible)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public void DisplayLevelHighscoreInput(string levelName, int score)
    {
        messageDisplay.text = levelMessageText;
        subMessageDisplay.text = levelName;
        scoreDisplay.value = score;
        inputField.onSubmit.AddListener(SubmitName);
        ShowUI();
        //StartCoroutine(SubmitLevelHighscoreNameCoroutine(levelIndex));
    }

    public void DisplayArcadeHighscoreInput(int score, int rank)
    {
        messageDisplay.text = rank == 0 ? arcadeBestMessageText : arcadeMessageText;
        subMessageDisplay.text = rank == 0 ? arcadeBestSubMessageText : arcadeSubMessageText;
        scoreDisplay.value = score;
        inputField.onSubmit.AddListener(SubmitName);
        ShowUI();
        //StartCoroutine(SubmitArcadeHighscoreNameCoroutine(rank));
    }

    private void SubmitName(string input)
    {
        inputField.onSubmit.RemoveListener(SubmitName);
        onSubmitName.Invoke(input);
        HideUI();
    }
}
