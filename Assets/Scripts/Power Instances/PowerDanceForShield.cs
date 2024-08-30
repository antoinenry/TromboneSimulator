using UnityEngine;

public class PowerDanceForShield : PowerDance
{
    [Header("Shielding")]
    public Shield shield;
    public GameObject shieldFrameGUI;
    public GameObject shieldIntegrityGUI;

    protected override void Awake()
    {
        base.Awake();
        // Shield initialization
        if (shield && perfJudge)
        {
            shield.performanceJudge = perfJudge;
            if (shield.shieldCrasher && perfJudge.noteCrasher)
            {
                shield.shieldCrasher.cam = perfJudge.noteCrasher.cam;
                shield.shieldCrasher.spawner = perfJudge.noteCrasher.spawner;
            }
        }
        // Additional GUI dispatch
        PerformanceJudgeGUI perfGUI = perfJudge != null ? perfJudge.gui : null;
        if (perfGUI != null && shieldFrameGUI) GUIDispatch.Dispatch(shieldFrameGUI.transform, perfGUI.transform, worldPositionStays:false);
        RectTransform perfPanel = perfGUI != null ? perfGUI.performancePanel : null;
        if (shieldIntegrityGUI != null && perfPanel != null) GUIDispatch.Dispatch(shieldIntegrityGUI.transform, perfPanel);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (shield)
        {
            shield.onActivate.AddListener(OnShieldUp);
            shield.onShieldDown.AddListener(OnShieldDown);
            shield.onDamaged.AddListener(OnShieldDamage);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (shield)
        {
            shield.onActivate.RemoveListener(OnShieldUp);
            shield.onShieldDown.RemoveListener(OnShieldDown);
            shield.onDamaged.RemoveListener(OnShieldDamage);
        }
    }

    protected override void OnDanceCountUp(int value, int maxValue)
    {
        base.OnDanceCountUp(value, maxValue);
        if (perfJudge == null || perfJudge.DetectionEnabled == false) return;
        onCharge.Invoke(value, maxValue);
        OnPowerEffect();
    }

    protected override void OnPowerEffect()
    {
        if (ChargeLevel >= MaxChargeLevel)
        {
            if (shield != null && shield.shieldUp == false)
            {
                shield.Activate();
                particleEffect?.Play();
            }
        }
        else
        {
            foreach (TintFlash tintEffect in tromboneTintEffects) tintEffect?.Tint(tromboneTintColor);
        }
    }

    private void OnShieldUp()
    {
        GUI?.SetActive(false);
    }

    private void OnShieldDown()
    {
        danceCounter.DanceCount = 0;
        GUI?.SetActive(true);
    }

    private void OnShieldDamage() => PlayTromboneTintEffect();
}
