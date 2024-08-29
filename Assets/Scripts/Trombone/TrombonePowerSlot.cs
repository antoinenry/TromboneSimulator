using UnityEngine;

public class TrombonePowerSlot : MonoBehaviour
{
    public GameObject powerObject;

    private void Awake()
    {
        if (powerObject != null)
        {
            if (powerObject.transform.parent != transform) powerObject = Instantiate(powerObject, transform);
        }
    }

    public void LoadPower(GameObject powerPrefab)
    {
        if (powerObject != null && powerObject.transform.parent == transform) Destroy(powerObject);
        if (powerPrefab != null) powerObject = Instantiate(powerPrefab, transform);
        else powerObject = null;
    }
}