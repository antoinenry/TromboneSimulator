using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Shield : MonoBehaviour
{
    [Header("Components")]
    public PerformanceJudge performanceJudge;
    public NoteCrasher shieldCrasher;
    public RectTransform shieldFrame;
    public SliderScaler integrityBar;
    [Header("Shielding")]
    public bool shieldUp = false;
    public float integrity = 0f;
    public float maxIntegrity = 1f;
    public float damageRate = 1f;
    [Header("Charge")]
    public float rechargeRate = 1f;
    public float rechargeDelay = 1f;
    public float activatingChargeRate = 10f;
    [Header("Events")]
    public UnityEvent onActivate;
    public UnityEvent onCharge;
    public UnityEvent onDamaged;
    public UnityEvent onShieldDown;

    private PerformanceJudgeGUI perfGUI;
    private NoteCrasher regularCrasher;
    private TintFlash shieldFrameEffect;
    private SpriteRenderer shieldFrameRenderer;
    private float chargeTimer;
    private bool isActivating;


    private void Awake()
    {
        if (shieldFrame)
        {
            shieldFrameEffect = shieldFrame.GetComponent<TintFlash>();
            shieldFrameRenderer = shieldFrame.GetComponent<SpriteRenderer>();
        }
        if (performanceJudge)
        {
            regularCrasher = performanceJudge.noteCrasher;
            perfGUI = performanceJudge.gui;
        }
    }

    private void OnEnable()
    {
        if (shieldCrasher)
        {
            shieldCrasher.spawner = regularCrasher.spawner;
            shieldCrasher.cam = regularCrasher.cam;
            shieldCrasher.onHorizontalCrash.AddListener(OnShieldCrash);
            shieldCrasher.onVerticalCrash.AddListener(OnShieldCrash);
        }
    }

    private void OnDisable()
    {
        if (shieldCrasher)
        {
            shieldCrasher.onHorizontalCrash.RemoveListener(OnShieldCrash);
            shieldCrasher.onVerticalCrash.RemoveListener(OnShieldCrash);
        }
        shieldUp = false;
        isActivating = false;
        UpdateCrashers();
        UpdateGUI();
    }

    private void Update()
    {
        UpdateGUI();
        UpdateCrashers();
        UpdateCharge();
    }

    private void UpdateGUI()
    {
        bool upGUI = shieldUp || isActivating;
        if (perfGUI == null || perfGUI.GUIActive == false) upGUI = false;
        if (shieldFrameRenderer) shieldFrameRenderer.enabled = upGUI;
        if (integrityBar)
        {
            integrityBar.gameObject.SetActive(upGUI);
            integrityBar.SetValueAndMax(integrity, maxIntegrity);
        }
    }

    private void UpdateCrashers()
    {
        if (regularCrasher) regularCrasher.enabled = !shieldUp;
        if (shieldCrasher) shieldCrasher.enabled = shieldUp;
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
                if (chargeTimer > rechargeDelay)
                {
                    integrity += rechargeRate * deltaTime;
                    onCharge.Invoke();
                }
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
                onActivate.Invoke();
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

    private void OnShieldCrash(float deltaTime)
    {
        // Tint effect
        if (shieldFrameEffect) shieldFrameEffect.Tint(); 
        // Invicibility period on activation
        if (isActivating) return;
        // Shield damage
        chargeTimer = 0f;
        if (integrity > 0f)
        {
            integrity -= deltaTime * damageRate;
            onDamaged.Invoke();
        }
        // Shield break
        if (integrity < 0f) Deactivate();
    }

    public void Activate()
    {
        if (shieldUp == false) isActivating = true;
    }

    public void Deactivate()
    {
        integrity = 0f;
        shieldUp = false;
        isActivating = false;
        onShieldDown.Invoke();
    }
}
