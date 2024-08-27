using UnityEngine;
using UnityEngine.Events;

public class PowerDanceForCombo : PowerDance
{
    [Header("Events")]
    public UnityEvent onComboIncrease;

    protected override void OnDanceCountUp(int value, int maxValue)
    {
        base.OnDanceCountUp(value, maxValue);
        if (perfJudge == null || perfJudge.DetectionEnabled == false) return;
        onCharge.Invoke(value, maxValue);
        if (ChargeLevel < MaxChargeLevel) return;
        perfJudge?.IncrementCombo();
        onComboIncrease.Invoke();
        OnPowerEffect();
    }
}
