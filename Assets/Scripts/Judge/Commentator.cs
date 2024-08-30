using UnityEngine;
using static CommentSheet;
using static NoteSpawn;

public class Commentator : MonoBehaviour
{
    public CommentSheet sheet;
    public float commentDuration = .5f;
    public bool highPriority;

    private LevelLoader levelLoader;
    private PerformanceJudge perfJudge;
    private LevelGUI gui;
    private int correctNoteCounter;
    private int missedNoteCounter;
    private float scoreCounter;

    private void Awake()
    {
        levelLoader = FindObjectOfType<LevelLoader>(true);
        perfJudge = GetComponent<PerformanceJudge>();
        gui = FindObjectOfType<LevelGUI>(true);
    }

    private void OnEnable()
    {
        AddListeners();
        ResetCounters();
    }

    private void OnDisable() => RemoveListeners();

    private void AddListeners()
    {
        if (levelLoader)
        {
            levelLoader.onStartLoadLevel.AddListener(OnLoadLevel);
        }
        if (perfJudge)
        {
            perfJudge.onNotePerformanceEnd.AddListener(OnNoteEnd);
            perfJudge.onHealth.AddListener(OnHealth);
            perfJudge.onNoteCombo.AddListener(OnCombo);
            perfJudge.onScore.AddListener(OnScore);
        }
    }

    private void RemoveListeners()
    {
        if (levelLoader)
        {
            levelLoader.onStartLoadLevel.RemoveListener(OnLoadLevel);
        }
        if (perfJudge)
        {
            perfJudge.onNotePerformanceEnd.RemoveListener(OnNoteEnd);
            perfJudge.onHealth.RemoveListener(OnHealth);
            perfJudge.onNoteCombo.RemoveListener(OnCombo);
            perfJudge.onScore.RemoveListener(OnScore);
        }
    }

    public void DisplayComment(string comment)
    {
        if (gui == null || comment == null || comment == string.Empty) return;
        if (highPriority || gui.CurrentMessage == null || gui.CurrentMessage == string.Empty)
            gui.ShowMessage(comment, commentDuration);
    }

    public void ResetCounters()
    {
        correctNoteCounter = 0;
        missedNoteCounter = 0;
        scoreCounter = 0f;
    }

    private void OnLoadLevel(Level l)
    {
        ResetCounters();
    }

    private void OnNoteEnd(NoteSpawn note, float points)
    {
        if (sheet == null || note == null) return;
        if (note.catchState == CatchState.All) DisplayComment(sheet.GetComment(CommentType.CorrectNote, ++correctNoteCounter, 1));
        else DisplayComment(sheet.GetComment(CommentType.MissNote, ++missedNoteCounter, 1));
    }

    private void OnHealth(float healthValue, float maxHealth, float healthDelta)
    {
        if (sheet == null) return;
        DisplayComment(sheet.GetComment(CommentType.Health, healthValue, healthDelta));
    }

    private void OnCombo(int comboValue)
    {
        if (sheet == null) return;
        DisplayComment(sheet.GetComment(CommentType.Combo, comboValue, 1));
    }

    private void OnScore(float scoreValue)
    {
        if (sheet == null) return;
        DisplayComment(sheet.GetComment(CommentType.Score, scoreValue, scoreValue - scoreCounter));
        scoreCounter = scoreValue;
    }
}
