using UnityEngine;
using System.Collections;

public class TransformShaker : MonoBehaviour
{
    public float amplitude;
    public float frequency;
    public float holdDuration;

    private Vector3 idlePosition;
    private bool shaking;
    private float holdTimer;

    private void Awake()
    {
        idlePosition = transform.localPosition;
    }

    public void Shake()
    {
        if (shaking == false)
        {
            shaking = true;
            StartCoroutine(ShakeCoroutine());
        }
        holdTimer = 0f;
    }

    public void Stop()
    {
        StopAllCoroutines();
        shaking = false;
        transform.localPosition = idlePosition;
    }

    private IEnumerator ShakeCoroutine()
    {
        float freqTimer = 0f;
        float deltaTime = 0f;
        idlePosition = transform.localPosition;
        while (shaking)
        {
            freqTimer += deltaTime;
            holdTimer += deltaTime;
            if (frequency != 0f && freqTimer * frequency > 1f)
            {
                transform.localPosition = idlePosition + new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude), 0f);
                freqTimer -= 1f / frequency;
            }
            yield return null;
            deltaTime = Time.deltaTime;
            if (holdTimer > holdDuration) shaking = false;
        }
        transform.localPosition = idlePosition;
    }
}
