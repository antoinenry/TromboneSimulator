using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAnimator : MonoBehaviour
{
    [Range(0f, 1f)] public float progress;
    public bool local = true;
    public Vector2 startPosition;
    public Vector2 endPosition;

    public void Animate(float animationProgress)
    {
        progress = Mathf.Clamp01(animationProgress);
        Vector2 pos = Vector2.Lerp(startPosition, endPosition, progress);
        if (local) transform.localPosition = pos;
        else transform.position = pos;
    }

    public void Animate(float from, float to)
    {
        Animate(to);
    }
}
