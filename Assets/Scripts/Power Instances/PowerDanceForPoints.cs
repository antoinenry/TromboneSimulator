using UnityEngine;
using UnityEngine.Events;

public class PowerDanceForPoints : PowerDance
{
    [Header("Configuration")]
    public int pointsPerChargeLevel = 10;
    [Header("Events")]
    public UnityEvent<int> onPoints;

    protected override void OnChargeUp()
    {
        base.OnChargeUp();
        if (perfJudge == null) return;
        int points = pointsPerChargeLevel * ChargeLevel;
        perfJudge.AddScore(points);
        onPoints.Invoke(points);
    }
}
