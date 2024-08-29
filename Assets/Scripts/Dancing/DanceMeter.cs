using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class DanceMeter : MonoBehaviour
{
    public bool startActive;
    [Header("Components")]
    public DanceDetector dance;
    public DanceMeterGUI GUI;
    [Header("Configuration")]
    public int maxBeats = 16;
    public int spawnDurationBeats = 20;
    public int missedBeatsBuffer = 1;
    public int beatsPerLevel = 4;
    public int pointsPerLevel = 500;
    [Header("Progress")]
    public int danceBeatLevel = 0;
    public int lifeTimeBeats = 0;
    [Header("Events")]
    public UnityEvent<int> onEnd;
    public UnityEvent onFull;

    public int consecutiveMissedBeats;

    public int Points => beatsPerLevel != 0 ? (danceBeatLevel / beatsPerLevel) * pointsPerLevel : 0;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            GUI?.PlayStartAnimation();
            lifeTimeBeats = 0;
            danceBeatLevel = 0;
            FindDancer();
            AddDanceListeners();
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            if (GUI) GUI.PlayEndAnimation(Points, danceBeatLevel == maxBeats);
            lifeTimeBeats = 0;
            danceBeatLevel = 0;
            RemoveDanceListeners();
        }
    }

    private void Update()
    {
        UpdateGUI();
    }

    private void FindDancer()
    {
        if (dance == null) return;
        TromboneDisplay trombone = FindObjectOfType<TromboneDisplay>(true);
        dance.dancer = trombone?.body?.transform;
    }

    private void AddDanceListeners()
    {
        dance?.onDanceBeat?.AddListener(OnDanceBeat);
        dance?.onMissBeat?.AddListener(OnMissBeat);
    }

    private void RemoveDanceListeners()
    {
        dance?.onDanceBeat?.RemoveListener(OnDanceBeat);
        dance?.onMissBeat?.RemoveListener(OnMissBeat);
    }

    private void UpdateGUI()
    {
        if (GUI == null) return;
        GUI.gaugeSize = maxBeats;
        GUI.gaugeValue = danceBeatLevel;
        GUI.timeValue = spawnDurationBeats > 0f ? Mathf.Clamp(lifeTimeBeats, 0f, spawnDurationBeats) / spawnDurationBeats : 1f;
        int levelCount = beatsPerLevel > 0 ? maxBeats / beatsPerLevel : 0;
        GUI.LevelCount = levelCount;
        for (int l = 0; l < levelCount; l++) GUI.levels[l] = (l + 1) * pointsPerLevel;
        GUI.Update();
    }

    private void OnBeat()
    {
        if (++lifeTimeBeats >= spawnDurationBeats || danceBeatLevel >= maxBeats) OnEnd();
    }

    private void OnDanceBeat()
    {
        consecutiveMissedBeats = 0;
        danceBeatLevel = Mathf.Clamp(danceBeatLevel + 1, 0, maxBeats);
        OnBeat();
    }

    private void OnMissBeat()
    {
        if (consecutiveMissedBeats++ > missedBeatsBuffer)
            danceBeatLevel = Mathf.Clamp(danceBeatLevel - 1, 0, maxBeats);
        OnBeat();
    }

    private void OnEnd()
    {
        onEnd.Invoke(danceBeatLevel);
        if (danceBeatLevel >= maxBeats) onFull.Invoke();
        enabled = false;        
    }
}
