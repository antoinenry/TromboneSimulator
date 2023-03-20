using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class TintFlash : MonoBehaviour
{
    public Color idleColor = Color.white;
    public Color tintColor = Color.red;
    public float holdDuration;
    public bool tint;

    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Image UIImage;
    private float holdTimer;

    public Color Color
    {
        get
        {
            if (cam != null) return cam.backgroundColor;
            if (spriteRenderer != null) return spriteRenderer.color;
            if (UIImage != null) return UIImage.color;
            return Color.clear;
        }
        set
        {
            if (cam != null) cam.backgroundColor = value;
            if (spriteRenderer != null) spriteRenderer.color = value;
            if (UIImage != null) UIImage.color = value;
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UIImage = GetComponent<Image>();
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

    public void Tint()
    {
        if (tint == false) StartCoroutine(TintCoroutine());
        holdTimer = 0f;
    }

    public void Stop()
    {
        StopAllCoroutines();
        tint = false;
    }

    private IEnumerator TintCoroutine()
    {
        tint = true;
        float deltaTime = 0f;
        float fadeSpeed = holdDuration != 0f ? 1f / holdDuration : 1f;
        while (tint)
        {
            holdTimer += deltaTime;
            Color = Color.Lerp(tintColor, idleColor, holdTimer * fadeSpeed);
            yield return null;
            deltaTime = Time.deltaTime;
            if (holdTimer > holdDuration) tint = false;
        }
        Color = idleColor;
    }
}
