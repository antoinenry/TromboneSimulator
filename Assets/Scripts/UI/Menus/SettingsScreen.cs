using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteAlways]
public class SettingsScreen : MenuUI
{
    //public GameState_old gameState;
    //public GameSettingsInfo settings;

    //[Header("UI Components")]
    //public Button touchControlsButton;
    //public Button mouseControlsButton;
    //public Slider backingVolumeSlider;
    //public Slider gridLatencySlider;
    //public Button backButton;

    //public UnityEvent onGoBack;

    //protected override void Awake()
    //{
    //    base.Awake();
    //    UISettings = this;
    //    if (Application.isPlaying)
    //    {
    //        LevelLoader levelLoader = FindObjectOfType<LevelLoader>(true);
    //        if (levelLoader != null && levelLoader.gameState) gameState = levelLoader.gameState;
    //    }
    //    if (gameState != null) settings = gameState.Settings;
    //}

    //public override void ShowUI()
    //{
    //    base.ShowUI();
    //    if (Application.isPlaying)
    //    {
    //        touchControlsButton.onClick.AddListener(SwitchToTouchControls);
    //        mouseControlsButton.onClick.AddListener(SwitchToMouseControls);

    //        backingVolumeSlider.value = settings.backingVolume;
    //        backingVolumeSlider.onValueChanged.AddListener(SetBackingVolume);

    //        gridLatencySlider.value = settings.latencyCorrection;
    //        gridLatencySlider.onValueChanged.AddListener(SetGridLatency);

    //        backButton.onClick.AddListener(GoBack);
    //    }
    //}
    //public override void HideUI()
    //{
    //    base.HideUI();
    //    if (Application.isPlaying)
    //    {
    //        touchControlsButton.onClick.RemoveListener(SwitchToTouchControls);
    //        mouseControlsButton.onClick.RemoveListener(SwitchToMouseControls);
    //        backingVolumeSlider.onValueChanged.RemoveListener(SetBackingVolume);
    //        gridLatencySlider.onValueChanged.RemoveListener(SetGridLatency);
    //        backButton.onClick.RemoveListener(GoBack);
    //    }
    //}

    //protected override void Update()
    //{
    //    base.Update();
    //    if (IsVisible)
    //    {
    //        if (gameState != null) gameState.Settings = settings;
    //        if (mouseControlsButton != null) mouseControlsButton.interactable = settings.useTouchControls;
    //        if (touchControlsButton != null) touchControlsButton.interactable = !settings.useTouchControls;
    //    }
    //}

    //private void SwitchToTouchControls()
    //{
    //    settings.useTouchControls = true;
    //}

    //private void SwitchToMouseControls()
    //{
    //    settings.useTouchControls = false;
    //}

    //private void SetBackingVolume(float volume)
    //{
    //    settings.backingVolume = volume;
    //}

    //private void SetGridLatency(float latency)
    //{
    //    settings.latencyCorrection = latency;
    //}

    //private void GoBack()
    //{        
    //    onGoBack.Invoke();
    //    onGoBack.RemoveAllListeners();
    //    HideUI();
    //}
}
