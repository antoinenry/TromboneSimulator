using UnityEngine;

public class PowerDanceForCombo : PowerDance
{
    protected override void OnChargeUp()
    {
        base.OnChargeUp();
        if (perfJudge == null) return;
        if (ChargeLevel >= MaxChargeLevel) perfJudge.IncrementCombo();
    }
}
