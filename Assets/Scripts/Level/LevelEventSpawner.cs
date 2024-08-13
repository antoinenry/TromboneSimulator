using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelEventSpawner : MonoBehaviour
{
    public bool showDebug = false;
    [Header("Components")]
    public Playhead musicPlayhead;
    public LevelEventPlayhead eventPlayhead;
    public LevelEventInstance[] eventPrefabs;

    public LevelEventInstance[] LoadedEvents {  get; private set; }

    private void OnEnable()
    {
        musicPlayhead?.onMove?.AddListener(MovePlayhead);
        eventPlayhead?.onEnterRead?.AddListener(StartEvent);
        eventPlayhead?.onExitRead?.AddListener(EndEvent);
    }

    private void OnDisable()
    {
        musicPlayhead?.onMove?.RemoveListener(MovePlayhead);
        eventPlayhead?.onEnterRead?.RemoveListener(StartEvent);
        eventPlayhead?.onExitRead?.RemoveListener(EndEvent);
    }

    public void LoadEvents(params LevelEventSheet[] sheets)
    {
        List<LevelEventInstance> loadedEvents = new List<LevelEventInstance>();
        foreach (LevelEventSheet sheet in sheets)
        {
            if (sheet == null) continue;
            Type instanceType = sheet.EventInstanceType;
            LevelEventInstance instancePrefab = GetEventPrefab(instanceType);
            if (instancePrefab == null) continue;
            ITimingInfo[] getEvents = sheet.GetEvents();
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
    }

    public void UnloadEvents()
    {
        if (LoadedEvents == null) return;
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

    private LevelEventInstance GetEventPrefab(Type eventInstanceType)
    {
        if (eventPrefabs == null) return null;
        if (eventInstanceType == null || eventInstanceType.IsAssignableFrom(typeof(LevelEventInstance))) return null;
        return Array.Find(eventPrefabs, p => p?.GetType() == eventInstanceType);
    }
}