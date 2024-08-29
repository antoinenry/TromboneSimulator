using UnityEngine;
using UnityEngine.Events;

public class PowerDanceForCombo : PowerDance
{
    [Header("Combo")]
    public int comboPerChargeLevel = 1;
    [Header("Events")]
    public UnityEvent<int> onAddCombo;

    protected override void OnDanceCountUp(int value, int maxValue)
    {
        base.OnDanceCountUp(value, maxValue);
        if (perfJudge == null || perfJudge.DetectionEnabled == false) return;
        onCharge.Invoke(value, maxValue);
        int addCombo = comboPerChargeLevel * ChargeLevel;
        perfJudge?.AddCombo(addCombo);
        onAddCombo.Invoke(addCombo);
        OnPowerEffect();
    }
}
