using UnityEngine;

public class TrombonePowerSlot : MonoBehaviour
{
    public GameObject power;

    private void Awake()
    {
        if (power != null)
        {
            if (power.transform.parent != transform) power = Instantiate(power, transform);
        }
    }

    public void LoadPower(GameObject powerPrefab)
    {
        if (power != null) Destroy(powerPrefab);
        if (powerPrefab != null) power = Instantiate(power, transform);
        else power = null;
    }
}