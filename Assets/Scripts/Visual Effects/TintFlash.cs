using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class TintFlash : MonoBehaviour
{
    public Color idleColor = Color.white;
    public Color tintColor = Color.red;
    public AnimationCurve curve = AnimationCurve.Linear(0.0f, 1f, 1f, 0f);
    public bool tint;

    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Image UIImage;
    private TMP_Text textfield;
    private float animationTimer;

    public Color Color
    {
        get
        {
            if (cam != null) return cam.backgroundColor;
            if (spriteRenderer != null) return spriteRenderer.color;
            if (UIImage != null) return UIImage.color;
            if (textfield != null) return textfield.color;
            return Color.clear;
        }
        set
        {
            if (cam != null) cam.backgroundColor = value;
            if (spriteRenderer != null) spriteRenderer.color = value;
            if (UIImage != null) UIImage.color = value;
            if (textfield != null) textfield.color = value;
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UIImage = GetComponent<Image>();
        textfield = GetComponent<TMP_Text>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Color = idleColor;
        tint = false;
    }

    private void Update()
    {
        if (Application.isPlaying == false) Color = tint ? tintColor : idleColor;
    }

    public float GetAnimationDuration()
    {
        Keyframe[] animationKeys = curve.keys;
        int animationLength = animationKeys != null ? animationKeys.Length : 0;
        if (animationLength == 0) return 0f;
        return animationKeys[animationLength - 1].time;
    }

    public void Tint()
    {
        if (tint == false)
        {
            StartCoroutine(TintCoroutine());
        }
        else
        {
            animationTimer = 0f;
            Color = tintColor;
        }
    }

    public void Tint(Color t)
    {
        tintColor = t;
        Tint();
    }

    public void Stop()
    {
        StopAllCoroutines();
        tint = false;
    }

    private IEnumerator TintCoroutine()
    {
        animationTimer = 0f;
        Color = tintColor;
        tint = true;
        float deltaTime = 0f;
        float duration = GetAnimationDuration();
        while (tint && duration > 0f && animationTimer <= duration)
        {
            Color = Color.Lerp(idleColor, tintColor, curve.Evaluate(animationTimer / duration));
            yield return null;
            deltaTime = Time.deltaTime;
            animationTimer += deltaTime;
        }
        Color = idleColor;
        tint = false;
    }
}
