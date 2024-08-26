using UnityEngine;

public class PowerDance : MonoBehaviour
{
    [Header("Base components")]
    public GameObject GUI;
    public ParticleSystem particleEffect;
    public DanceCounter danceCounter;
    [Header("Base look")]
    public Color tintEfectColor = Color.yellow;

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
        GUIDispatch?.DestroyDispatched();
    }

    protected virtual void Update()
    {
        particleEffect.transform.position = trombone.GrabPosition;
    }

    public virtual void SetChargeLevel(int value)
    {
        if (value > ChargeLevel) OnChargeUp();
        else if (value < ChargeLevel) OnChargeDown();
    }

    public virtual void SetChargeLevel(int value, int maxValue)
    {
        SetChargeLevel(value);
    }

    private void OnDanceCountUp(int value, int maxValue)
    {
        OnChargeUp();
    }

    private void OnDanceCountDown(int value, int maxValue)
    {
        OnChargeDown();
    }

    protected virtual void OnChargeUp()
    {
        foreach (TintFlash tintEffect in tromboneTintEffects) tintEffect.Tint(tintEfectColor);
        frameTintEffect.Tint(tintEfectColor);
        particleEffect.Play();
    }

    protected virtual void OnChargeDown()
    {
        particleEffect.Stop();
    }
}