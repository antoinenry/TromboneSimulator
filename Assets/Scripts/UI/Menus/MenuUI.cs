﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[ExecuteAlways]
public abstract class MenuUI : MonoBehaviour
{
    public RectTransform UI;
    public bool showUI;
    public bool startVisible;
    [Header("Events")]
    public UnityEvent onShowUI;
    public UnityEvent onHideUI;

    static public HandCursor cursor;
    static public List<MenuUI> visibleMenuUis;
    static public MainMenu UIMainMenu;
    static public LevelSelectionScreen UILevelSelection;
    static public LeaderBoardScreen UILeaderboard;
    static public PauseScreen UIPause;
    static public ScoreScreen UIScore;
    static public GameOverScreen UIGameOver;
    static public NewHighscoreScreen UIHighScore;
    static public SettingsScreen UISettings;
    static public LoadingScreen UILoading;

    public MenuUI PreviousUI { get; protected set; }

    static public int VisibleMenuCount => visibleMenuUis != null ? visibleMenuUis.Count : 0;

    public bool IsVisible { get; private set; }

    protected virtual void Reset()
    {
        if (transform.childCount > 0) UI = transform.GetChild(0).GetComponent<RectTransform>();
    }

    protected virtual void Awake()
    {
        if (cursor == null) cursor = FindObjectOfType<HandCursor>(true);
        if (visibleMenuUis == null) visibleMenuUis = new List<MenuUI>();
    }

    protected virtual void Start()
    {
        if (Application.isPlaying)
        {
            if (startVisible) ShowUI();
            else HideUI();
        }
    }

    protected virtual void Update()
    {
        if (IsVisible != showUI)
        {
            if (showUI) ShowUI();
            else HideUI();
        }
    }

    public virtual void ShowUI()
    {
        showUI = true;
        if (UI != null) UI.gameObject.SetActive(true);
        IsVisible = true;
        onShowUI.Invoke();
        if (Application.isPlaying)
        {
            if (visibleMenuUis.Contains(this) == false) visibleMenuUis.Add(this);
        }
    }

    public virtual void HideUI()
    {
        showUI = false;
        if (UI != null) UI.gameObject.SetActive(false);
        IsVisible = false;
        onHideUI.Invoke();
        if (Application.isPlaying)
        {
            visibleMenuUis.RemoveAll(m => m == this);
        }
    }

    public virtual void GoTo(MenuUI nextUI)
    {
        if (nextUI != null)
        {
            nextUI.ShowUI();
            nextUI.PreviousUI = this;
        }
        HideUI();
    }

    public virtual void GoBack()
    {
        if (PreviousUI != null)
        {
            PreviousUI.ShowUI();
            PreviousUI = null;
        }
        HideUI();
    }
}