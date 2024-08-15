using TMPro;
using UnityEngine;

[ExecuteAlways]
public class PlayNotesForPointsGUI : MonoBehaviour
{
    public enum NoteState { Default, Played, Missed, AllPlayed }

    [Header("Components")]
    public TMP_Text labelField;
    public NodeGauge gauge;
    public TMP_Text pointsField;
    [Header("Look")]
    public string pointsPrefix = "+";
    public float pointsDisplayDuration = 1f;
    [Header("Input")]
    [SerializeField] private int points = 1000;

    private float pointsDisplayTimer;

    private void Update()
    {
        if (Application.isPlaying) UpdatePointsAlpha();
    }

    public void SetGaugeLength(int length)
    {
        if (gauge == null) return;
        gauge.NodeCount = length;
    }

    public void SetNoteStateAt(int index, NoteState state)
    {
        gauge?.SetNodeAt(index, (int)state);        
    }

    public void SetAllNoteStates(NoteState state)
    {
        gauge?.SetAllNodes((int)state);
    }

    public void SetPoints(int value)
    {
        points = value;
        pointsDisplayTimer = pointsDisplayDuration;
        pointsField?.SetText(pointsPrefix + points);
    }

    private void UpdatePointsAlpha()
    {
        if (pointsField == null) return;
        pointsDisplayTimer -= Time.deltaTime;
        Color pointsColor = pointsField.color;
        pointsColor.a = pointsDisplayDuration != 0f ? pointsDisplayTimer / pointsDisplayDuration : 0f;
        pointsField.color = pointsColor;
    }
}