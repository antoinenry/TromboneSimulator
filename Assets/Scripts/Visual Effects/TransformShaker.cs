using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class TransformShaker : MonoBehaviour
{
    public float amplitude;
    public float frequency;
    public float holdDuration;
    public bool roundPosition = true;

    private RectTransform rectTransform;
    private Vector3 idlePosition;
    private bool shaking;
    private float holdTimer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        idlePosition = Position;
    }

    public Vector2 Position
    {
        get => rectTransform == null ? transform.localPosition : rectTransform.anchoredPosition;

        set
        {
            if (rectTransform != null) rectTransform.anchoredPosition = value;
            else transform.localPosition = value;
        }
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
        Position = idlePosition;
    }

    private IEnumerator ShakeCoroutine()
    {
        float freqTimer = 0f;
        float deltaTime = 0f;
        while (shaking)
        {
            freqTimer += deltaTime;
            holdTimer += deltaTime;
            if (frequency != 0f && freqTimer * frequency > 1f)
            {
                Vector3 shake = new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude));
                if (roundPosition) shake = new Vector3(Mathf.Round(shake.x), Mathf.Round(shake.y));
                Position = idlePosition + shake;
                freqTimer -= 1f / frequency;
            }
            yield return null;
            deltaTime = Time.deltaTime;
            if (holdTimer > holdDuration) shaking = false;
        }
        Position = idlePosition;
    }
}
