using UnityEngine;
using UnityEngine.Events;

public class PowerDance : MonoBehaviour
{
    [Header("Base components")]
    public GameObject GUI;
    public ParticleSystem particleEffect;
    public DanceCounter danceCounter;
    [Header("Base look")]
    public Color frameTintColor = Color.yellow;
    public Color tromboneTintColor = Color.yellow;
    [Header("Base Events")]
    public UnityEvent<int, int> onCharge;

    protected TromboneDisplay trombone;
    protected PerformanceJudge perfJudge;
    protected TintFlash[] tromboneTintEffects;
    protected TintFlash frameTintEffect;
    protected GameObjectDispatch GUIDispatch = new GameObjectDispatch();

    public int ChargeLevel => danceCounter != null ? danceCounter.DanceCount : 0;
    public int MaxChargeLevel => danceCounter != null ? danceCounter.MaxDanceCount : 0;

    protected virtual void Awake()
    {
        trombone = FindObjectOfType<TromboneDisplay>(true);
        tromboneTintEffects = trombone != null ? trombone.GetComponentsInChildren<TintFlash>(true) : null;
        perfJudge = FindObjectOfType<PerformanceJudge>(true);
        PerformanceJudgeGUI perfGUI = perfJudge != null ? perfJudge.gui : null;
        frameTintEffect = perfGUI != null ? perfGUI.frame : null;
        RectTransform perfPanel = perfGUI != null ? perfGUI.performancePanel : null;
        if (GUI != null && perfPanel != null) GUIDispatch.Dispatch(GUI.transform, perfPanel);
    }

    protected virtual void OnEnable()
    {
        if (GUI) GUI.SetActive(true);
        if (particleEffect) particleEffect.Stop();
        if (danceCounter)
        {
            danceCounter.onIncrease.AddListener(OnDanceCountUp);
            danceCounter.onDecrease.AddListener(OnDanceCountDown);
        }
        onCharge.Invoke(ChargeLevel, MaxChargeLevel);
    }

    protected virtual void OnDisable()
    {
        if (GUI) GUI.SetActive(false);
        if (particleEffect) particleEffect.Stop();
        if (danceCounter)
        {
            danceCounter.onIncrease.RemoveListener(OnDanceCountUp);
            danceCounter.onDecrease.RemoveListener(OnDanceCountDown);
        }
    }

    protected virtual void OnDestroy()
    {
        GUIDispatch.DestroyDispatched();
    }

    protected virtual void Update()
    {
        if (particleEffect) particleEffect.transform.position = trombone.GrabPosition;
    }

    protected virtual void OnDanceCountUp(int value, int maxValue) 
    {
        if (frameTintEffect) frameTintEffect.Tint(frameTintColor);
        if (danceCounter != null && perfJudge != null && perfJudge.DetectionEnabled == false)
            danceCounter.DanceCount = 0;
    }

    protected virtual void OnDanceCountDown(int value, int maxValue)
    {
        if (particleEffect) particleEffect.Stop();
        onCharge.Invoke(value, maxValue);
    }

    protected virtual void OnPowerEffect()
    {
        if (particleEffect) particleEffect.Play();
        PlayTromboneTintEffect();
    }

    protected virtual void PlayTromboneTintEffect()
    {
        if (tromboneTintEffects == null) return;
        foreach (TintFlash tintEffect in tromboneTintEffects) tintEffect?.Tint(tromboneTintColor);
    }
}