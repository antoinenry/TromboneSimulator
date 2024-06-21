using System;
using UnityEngine;
using UnityEngine.UI;

public class DanceForShield : DancePower
{
    public RectTransform shieldFrame;
    public Slider chargeGauge;
    public Slider integrityGauge;
    public bool shieldUp = false;
    public int beatsToActivate = 10;
    public float integrity = 0f;
    public float maxIntegrity = 1f;
    public float damageRate = 1f;
    public float activatingChargeRate = 2f;
    public float rechargeRate = 1f;
    public float rechargeDelay = 1f;

    private NoteCrasher shieldCrasher;
    private NoteCrasher regularCrasher;
    private TintFlash shieldFrameEffect;
    private SpriteRenderer shieldFrameRenderer;
    private int beatCounter;
    private float chargeTimer;
    private bool isActivating;


    protected override void Awake()
    {
        base.Awake();
        shieldCrasher = GetComponent<NoteCrasher>();
        regularCrasher = Array.Find(FindObjectsOfType<NoteCrasher>(true), c => c != shieldCrasher);
        shieldFrameEffect = shieldFrame.GetComponent<TintFlash>();
        shieldFrameRenderer = shieldFrame.GetComponent<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        shieldCrasher.spawner = regularCrasher.spawner;
        shieldCrasher.cam = regularCrasher.cam;
        shieldCrasher.onHorizontalCrash.AddListener(OnShieldCrash);
        shieldCrasher.onVerticalCrash.AddListener(OnShieldCrash);
        shieldCrasher.onHorizontalCrash.AddListener(OnRegularCrash);
        shieldCrasher.onVerticalCrash.AddListener(OnRegularCrash);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        shieldCrasher.onHorizontalCrash.RemoveListener(OnShieldCrash);
        shieldCrasher.onVerticalCrash.RemoveListener(OnShieldCrash);
        shieldCrasher.onHorizontalCrash.RemoveListener(OnRegularCrash);
        shieldCrasher.onVerticalCrash.RemoveListener(OnRegularCrash);
        shieldUp = false;
        isActivating = false;
        UpdateCrashers();
        UpdateGUI();
    }

    protected override void Update()
    {
        base.Update();
        UpdateGUI();
        UpdateCrashers();
        UpdateCharge();
    }

    private void UpdateGUI()
    {
        if (perfGUI.GUIActive)
        {
            bool upGUI = shieldUp || isActivating;
            shieldFrameRenderer.enabled = upGUI;
            integrityGauge.gameObject.SetActive(upGUI);
            chargeGauge.gameObject.SetActive(!upGUI);
            chargeGauge.maxValue = beatsToActivate;
            chargeGauge.value = beatCounter;
            integrityGauge.maxValue = maxIntegrity;
            integrityGauge.value = integrity;
        }
        else
        {
            shieldFrameRenderer.enabled = false;
            integrityGauge.gameObject.SetActive(false);
            chargeGauge.gameObject.SetActive(false);
        }
    }

    private void UpdateCrashers()
    {
        regularCrasher.enabled = !shieldUp;
        shieldCrasher.enabled = shieldUp;
    }

    private void UpdateCharge()
    {
        float deltaTime = Time.deltaTime;
        if (shieldUp)
        {
            // Recharge
            if (integrity < maxIntegrity)
            {
                chargeTimer += deltaTime;
                if (chargeTimer > rechargeDelay) integrity += rechargeRate * deltaTime;
            }
            // Fully charged
            else
                integrity = maxIntegrity;
        }
        else
        {
            // Shield was just activated, first charge
            if (isActivating)
            {                
                integrity += activatingChargeRate * deltaTime;
                particleEffect.Play();
                // Activating period complete
                if (integrity >= maxIntegrity)
                {
                    integrity = maxIntegrity;
                    isActivating = false;
                    shieldUp = true;
                }
            }
            // Shield is inactive
            else integrity = 0f;
        }
    }

    protected override void OnDanceBeat()
    {
        if (dance.DanceCounter > beatsToStart)
        {
            perfGUI.frame.Tint(frameTint);
            if (shieldUp)
            {

            }
            else
            {
                if (beatCounter++ > beatsToActivate)
                    isActivating = true;
            }
        }
    }

    protected override void OnMissBeat()
    {
        base.OnMissBeat();
        if (!shieldUp)
        {
            // Lose a beat
            if (beatCounter > 0) beatCounter--;
        }
    }

    private void OnShieldCrash(float deltaTime)
    {
        // Invicibility period on activation
        if (isActivating) return;
        // Shield damage
        chargeTimer = 0f;
        if (integrity > 0f) integrity -= deltaTime* damageRate;
        // Shield break
        if (integrity < 0f)
        {
            beatCounter = 0;
            integrity = 0f;
            shieldUp = false;
        }
    }

    private void OnRegularCrash(float deltaTime)
    {
        // Cancels beat counting
        beatCounter = 0;
    }
}
