using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class LevelEventSpawner : MonoBehaviour
{
    public bool showDebug = false;
    [Header("Components")]
    public Playhead musicPlayhead;
    public LevelEventPlayhead eventPlayhead;
    public LevelEventInstance[] eventPrefabs;
    [Header("Settings")]
    public float tempoModifier = 1f;

    public UnityEvent<LevelEventInstance,float> onEventCompletion;

    public LevelEventInstance[] LoadedEvents {  get; private set; }

    private void OnEnable()
    {
        musicPlayhead?.onMove?.AddListener(MovePlayhead);
        musicPlayhead?.onStop?.AddListener(EndAllEvents);
        eventPlayhead?.onEnterRead?.AddListener(StartEvent);
        eventPlayhead?.onExitRead?.AddListener(EndEvent);
        AddEventListeners();
    }

    private void OnDisable()
    {
        musicPlayhead?.onMove?.RemoveListener(MovePlayhead);
        musicPlayhead?.onStop?.RemoveListener(EndAllEvents);
        eventPlayhead?.onEnterRead?.RemoveListener(StartEvent);
        eventPlayhead?.onExitRead?.RemoveListener(EndEvent);
        RemoveEventListeners();
    }

    private void AddEventListeners()
    {
        if (LoadedEvents == null) return;
        foreach (LevelEventInstance e in LoadedEvents)
            e?.onCompletion?.AddListener(OnEventCompletion);
    }

    private void RemoveEventListeners()
    {
        if (LoadedEvents == null) return;
        foreach (LevelEventInstance e in LoadedEvents)
            e?.onCompletion?.RemoveListener(OnEventCompletion);

    }

    public void LoadEvents(params LevelEventSheet[] sheets)
    {
        if (enabled == false || sheets == null || sheets.Length == 0) return;
        // Create working copies
        LevelEventSheet[] sheetCopies = Array.ConvertAll(sheets, s => s != null ? Instantiate(s) : ScriptableObject.CreateInstance<LevelEventSheet>());
        // Alter copies
        foreach (LevelEventSheet s in sheetCopies)
        {
            s.name += " (copy)";
            s.MultiplyTempoBy(tempoModifier);
        }
        // Load events
        List<LevelEventInstance> loadedEvents = new List<LevelEventInstance>();
        if (LoadedEvents != null) loadedEvents.AddRange(LoadedEvents);
        foreach (LevelEventSheet sheet in sheetCopies)
        {
            if (sheet == null) continue;
            Type instanceType = sheet.EventInstanceType;
            LevelEventInstance instancePrefab = GetEventPrefab(instanceType);
            if (instancePrefab == null) continue;
            ITimingInfo[] getEvents = sheet.GetEventTimings();
            if (getEvents == null) continue;
            foreach(ITimingInfo e in getEvents)
            {
                LevelEventInstance instance = Instantiate(instancePrefab, transform);
                instance.SetEventInfo(e);
                instance.enabled = false;
                loadedEvents.Add(instance);
            }
        }
        LoadedEvents = loadedEvents.ToArray();
        AddEventListeners();
    }

    public void UnloadEvents()
    {
        if (LoadedEvents == null) return;
        RemoveEventListeners();
        foreach (LevelEventInstance loadedEventInstance in LoadedEvents)
        {
            if (loadedEventInstance == null) continue;
            Destroy(loadedEventInstance.gameObject);
        }
        LoadedEvents = null;
    }

    public void MovePlayhead(float fromTime, float toTime)
    {
        if (eventPlayhead == null) return;
        eventPlayhead.Move(LoadedEvents, fromTime, toTime);
    }

    public void StartEvent(int eventIndex, LevelEventInstance eventInstance)
    {
        if (showDebug) Debug.Log("Start level event " + eventIndex + " - " + eventInstance?.name);
        eventInstance?.StartEvent();
    }

    public void EndEvent(int eventIndex, LevelEventInstance eventInstance)
    {
        if (showDebug) Debug.Log("End level event " + eventIndex + " - " + eventInstance?.name);
        eventInstance?.EndEvent();
    }

    public void EndAllEvents()
    {
        if (LoadedEvents == null) return;
        foreach (LevelEventInstance eventInstance in LoadedEvents) eventInstance?.EndEvent();
    }

    private LevelEventInstance GetEventPrefab(Type eventInstanceType)
    {
        if (eventPrefabs == null) return null;
        if (eventInstanceType == null || eventInstanceType.IsAssignableFrom(typeof(LevelEventInstance))) return null;
        return Array.Find(eventPrefabs, p => p?.GetType() == eventInstanceType);
    }

    private void OnEventCompletion(LevelEventInstance eventInstance, float completion)
    {
        if (eventInstance == null) return;
        onEventCompletion?.Invoke(eventInstance, completion);
    }
}