using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;

[ExecuteAlways]
public class DanceForPointsGUI : MonoBehaviour
{
    [Serializable]
    public struct ColorSheme
    {
        public Color labelColor;
        public Color gaugeColor;
        public Color gaugeFillColor;
        public Color pointsColor;

        public static ColorSheme AllWhite => new ColorSheme()
        {
            labelColor = Color.white, gaugeColor = Color.white, pointsColor = Color.white, gaugeFillColor = Color.white
        };
    }

    [Header("Components")]
    public TMP_Text labelField;
    public SliderScaler danceGauge;
    public Image danceGaugeImage;
    public Image danceGaugeFill;
    public TMP_Text pointsField;
    [Header("Look")]
    public string pointsPrefix = "+";
    public float pointsDisplayDuration = .5f;
    public ColorSheme dancingColor = ColorSheme.AllWhite;
    public ColorSheme missingColor = ColorSheme.AllWhite;
    public ColorSheme maxedColors = ColorSheme.AllWhite;
    [Header("Input")]
    [SerializeField] private string label = "Dance";
    [SerializeField] private int danceValue = 1;
    [SerializeField] private int maxDanceValue = 64;
    [SerializeField] private int points = 1000;

    private float pointsDisplayTimer;
    private bool isDecreasing;

    private void Update()
    {
        UpdateColorSheme();
        if (Application.isPlaying == false) UpdateGUI();
        else UpdatePointsAlpha();
    }

    private void UpdateGUI()
    {
        SetLabel(label);
        SetGauge(danceValue, maxDanceValue);
        SetPoints(points);
    }

    public int DanceValue
    {
        get => danceValue;
        set => SetGauge(value);
    }

    public void SetLabel(string value = null)
    {
        if (value != null) label = value;
        labelField?.SetText(label);
    }

    public void SetGauge(int value = -1, int maxValue = -1)
    {
        // Values
        if (value >= 0)
        {
            isDecreasing = value < danceValue;
            danceValue = value;
        }
        if (maxValue >= 0) maxDanceValue = maxValue;
        // Slider values
        if (danceGauge)
        {
            danceGauge.maxValue = maxDanceValue;
            danceGauge.value = danceValue;
        }
    }

    public void SetPoints(int value)
    {
        points = value;
        pointsDisplayTimer = pointsDisplayDuration;
        pointsField?.SetText(pointsPrefix + points);
    }

    private void UpdatePointsAlpha()
    {
        if (pointsField == null) return;
        pointsDisplayTimer -= Time.deltaTime;
        Color pointsColor = pointsField.color;
        pointsColor.a = pointsDisplayDuration != 0f ? pointsDisplayTimer / pointsDisplayDuration : 0f;
        pointsField.color = pointsColor;
    }

    public void UpdateColorSheme()
    {
        if (isDecreasing) SetColors(missingColor);
        else SetColors(danceValue < maxDanceValue ? dancingColor : maxedColors);
    }

    private void SetColors(ColorSheme colors)
    {
        if (labelField) labelField.color = colors.labelColor;
        if (danceGaugeImage) danceGaugeImage.color = colors.gaugeColor;
        if (danceGaugeFill) danceGaugeFill.color = colors.gaugeFillColor;
        if (pointsField) pointsField.color = colors.pointsColor;
    }
}
