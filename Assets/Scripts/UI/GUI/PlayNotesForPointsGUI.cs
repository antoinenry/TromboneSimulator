using TMPro;
using UnityEngine;

[ExecuteAlways]
public class PlayNotesForPointsGUI : MonoBehaviour
{
    public enum NoteState { Default, Played, Missed, AllPlayed }

    [Header("Components")]
    public TMP_Text labelField;
    public NodeGauge gauge;
    public CounterDisplay pointsfield;
    [Header("Input")]
    [SerializeField] private int points = 1000;

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
        pointsfield?.SetValue(points);
    }
}