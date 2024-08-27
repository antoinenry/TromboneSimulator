using UnityEngine.Events;
using UnityEngine;

public class PowerDanceForHealing : PowerDance
{
    [Header("Healing")]
    public GameObject[] deactivatedOnFullHealth;
    public float healPointsPerChargeLevel = .01f;
    public UnityEvent<float> onHeal;

    protected override void Update()
    {
        base.Update();
        if (deactivatedOnFullHealth != null)
        {
            bool activate = (perfJudge != null && perfJudge.health < perfJudge.maxHealth);
            foreach (GameObject go in  deactivatedOnFullHealth) go?.SetActive(activate);
        }
    }

    protected override void OnDanceCountUp(int value, int maxValue)
    {
        base.OnDanceCountUp(value, maxValue);
        if (perfJudge == null || perfJudge.DetectionEnabled == false) return;
        onCharge.Invoke(value, maxValue);
        if (perfJudge.health == perfJudge.maxHealth) return;
        float hp = healPointsPerChargeLevel * ChargeLevel;
        perfJudge.HealDamage(hp);
        onHeal.Invoke(hp);
        OnPowerEffect();            
    }
}
