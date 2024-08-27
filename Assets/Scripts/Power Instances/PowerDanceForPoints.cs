using UnityEngine;
using UnityEngine.Events;

public class PowerDanceForPoints : PowerDance
{
    [Header("Configuration")]
    public int pointsPerChargeLevel = 10;
    [Header("Events")]
    public UnityEvent<int> onPoints;

    protected override void OnDanceCountUp(int value, int maxValue)
    {
        base.OnDanceCountUp(value, maxValue);
        if (perfJudge == null || perfJudge.DetectionEnabled == false) return;
        onCharge.Invoke(value, maxValue);
        int points = pointsPerChargeLevel * ChargeLevel;
        perfJudge.AddScore(points);
        particleEffect?.Play();
        onPoints.Invoke(points);
        OnPowerEffect();
    }
}
