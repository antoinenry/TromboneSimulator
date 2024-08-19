using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class SliderBlinkAnimator : MonoBehaviour
{
    [Header("Components")]
    public Image blinkingElement;
    public Metronome metronome;
    [Header("Configuration")]
    public float threshold = .1f;
    public bool absoluteValue = false;
    public bool blinkWhenInferior = true;
    public float blinkingDuration = 1f;
    [Header("Events")]
    public UnityEvent<bool> onBlink;

    private Slider slider;
    private Coroutine blinkCoroutine;
    private bool isBlinking;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        slider?.onValueChanged?.AddListener(OnSliderValue);
    }

    private void OnDisable()
    {
        slider?.onValueChanged?.RemoveListener(OnSliderValue);
        StopBlinking();
    }

    private void StartBlinking()
    {
        if (isBlinking) return;
        if (metronome)
        {
            metronome.onBeatChange.AddListener(OnMetronomeBeat);
        }
        else
        {
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }
        isBlinking = true;
    }

    private void StopBlinking()
    {
        if (metronome != null) metronome.onBeatChange?.RemoveListener(OnMetronomeBeat);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
        isBlinking = false;
        BlinkElementVisibility = true;
    }

    private bool BlinkElementVisibility
    {
        get => blinkingElement != null ? blinkingElement.enabled : false;
        set
        {
            if (blinkingElement != null) blinkingElement.enabled = value;
        }
    }

    private void Blink()
    {
        BlinkElementVisibility = !BlinkElementVisibility;
        onBlink.Invoke(BlinkElementVisibility);
    }

    private void OnSliderValue(float value)
    {
        float absoluteThreshold = threshold;
        if (absoluteValue == false && slider != null && slider.maxValue != 0f) absoluteThreshold = threshold * slider.maxValue;
        bool shouldBlink = (blinkWhenInferior && value < absoluteThreshold) || (!blinkWhenInferior && value > absoluteThreshold);
        if (shouldBlink && !isBlinking) StartBlinking();
        else if (!shouldBlink && isBlinking) StopBlinking();
    }

    private IEnumerator BlinkCoroutine()
    {
        while (enabled && blinkCoroutine != null && isBlinking)
        {
            Blink();
            yield return new WaitForSeconds(blinkingDuration);
        }
        blinkCoroutine = null;
        isBlinking = false;
    }

    private void OnMetronomeBeat(int fromBeat, int toBeat)
    {
        if (isBlinking) Blink();
    }
}
