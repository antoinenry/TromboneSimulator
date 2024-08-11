using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[ExecuteAlways]
public class DanceMeterGUI : MonoBehaviour
{
    [Header("Components")]
    public RectTransform GUIRect;
    public Slider danceGauge;
    public Image timeGauge;
    public VerticalLayoutGroup levelLineLayout;
    public TMP_Text topLevelLine;
    public TMP_Text levelLinePrefab;
    public TMP_Text[] levelLineInstances;
    public TMP_Text totalPointsField;
    public Animation animate;
    [Header("Look")]
    public int heightPerGaugeSize = 4;
    public int minHeight = 1;
    public Color pointsColor = Color.green;
    public Color maxPointsColor = Color.yellow;
    public AnimationClip popInAnimation;
    public AnimationClip popOutAnimation;
    public AnimationClip fullPointsAnimation;
    public float gaugeAnimationSpeed = 10;
    public float radialAnimationSpeed = .25f;
    public float popOutAnimationDelay = 1f;
    [Header("Input")]
    public int gaugeValue = 4;
    public int gaugeSize = 8;
    [Range(0f, 1f)] public float timeValue = .5f;
    public int[] levels;
    public int levelSize = 4;

    private float animatedGaugeValue;
    private float animatedRadialValue;

    private void OnEnable()
    {
        if (danceGauge) danceGauge.value = 0;
        animatedGaugeValue = 0f;
        if (timeGauge) timeGauge.fillAmount = 0f;
        animatedRadialValue = 0f;
    }

    public void Update()
    {
        UpdateRect();
        UpdateDanceGauge();
        UpdateTimeGauge();
        UpdateLines();
    }

    public void PlayStartAnimation()
    {
        PlayAnimation(popInAnimation);
    }

    public void PlayEndAnimation(int pointsValue, bool isMax)
    {
        if (isMax)
        {
            SetTotalPointsField(pointsValue, maxPointsColor);
            PlayAnimation(fullPointsAnimation);
        }
        else
        {
            SetTotalPointsField(pointsValue, pointsColor);
        }
        if (enabled && gameObject.activeInHierarchy) PlayAnimation(popOutAnimation, popOutAnimationDelay);
        else PlayAnimation(popOutAnimation);

    }

    private void UpdateRect()
    {
        if (GUIRect == null) return;
        Vector2 sizeDelta = GUIRect.sizeDelta;
        sizeDelta.y = minHeight + heightPerGaugeSize * gaugeSize;
        GUIRect.sizeDelta = sizeDelta;
    }

    private void UpdateDanceGauge()
    {
        if (danceGauge == null) return;
        gaugeValue = Mathf.Clamp(gaugeValue, 0, gaugeSize);
        danceGauge.maxValue = gaugeSize * heightPerGaugeSize;
        if (Application.isPlaying && gaugeAnimationSpeed >= 0)
        {
            animatedGaugeValue = Mathf.Lerp(animatedGaugeValue, gaugeValue, gaugeAnimationSpeed * Time.deltaTime);
            danceGauge.value = animatedGaugeValue * heightPerGaugeSize;
        }
        else
        {
            danceGauge.value = gaugeValue * heightPerGaugeSize;
        }
    }

    private void UpdateTimeGauge()
    {
        if (timeGauge == null) return;
        timeValue = Mathf.Clamp(timeValue, 0f, 1f);
        if (Application.isPlaying && radialAnimationSpeed >= 0)
        {
            animatedRadialValue = Mathf.Lerp(animatedRadialValue, timeValue, radialAnimationSpeed * Time.deltaTime);
            timeGauge.fillAmount = animatedRadialValue;
        }
        else
        {
            timeGauge.fillAmount = timeValue;
        }
    }

    public int LevelCount
    {
        get => levels != null ? levels.Length : 0;
        set
        {
            if (LevelCount == value) return;
            levels = new int[Mathf.Max(0, value)];
        }
    }

    private void UpdateLines()
    {
        if (levelLineLayout == null) return;
        levelLineLayout.spacing = levelSize * heightPerGaugeSize;
        int lineCount = levelLineInstances != null ? levelLineInstances.Length : 0;
        int levelCount = LevelCount;
        if (lineCount != levelCount) ResetLineInstances();
        if (levelCount < 1) return;
        for (int i = 0; i < levelCount - 1; i++)
        {
            if (levelLineInstances[i] == null) continue;
            levelLineInstances[i].SetText(levels[i].ToString());
            levelLineInstances[i].color = pointsColor;
        }
        if (topLevelLine)
        {
            topLevelLine.SetText(levels[levelCount - 1].ToString());
            topLevelLine.color = maxPointsColor;
        }
    }

    private void ClearLines()
    {
        if (levelLineInstances == null) return;
        foreach (TMP_Text l in levelLineInstances) DestroyImmediate(l?.gameObject);
        levelLineInstances = null;
    }

    private void ResetLineInstances()
    {
        ClearLines();
        int levelCount = levels != null ? levels.Length : 0;
        levelLineInstances = new TMP_Text[levelCount];
        if (levelLineLayout == null) return;
        for (int i = 0; i < levelCount - 1; i++) levelLineInstances[i] = Instantiate(levelLinePrefab, levelLineLayout?.transform);
    }

    private void SetTotalPointsField(int value, Color color)
    {
        if (totalPointsField == null) return;
        if (value > 0)
        {
            totalPointsField.SetText(value.ToString());
            color.a = 0f;
            totalPointsField.color = color;
        }
        else
            totalPointsField.color = Color.clear;
    }

    private void PlayAnimation(AnimationClip clip, float delay = 0f)
    {
        if (animate == null) return;
        if (delay > 0f)
        {
            StartCoroutine(PlayAnimationCoroutine(clip, delay));
        }
        else
        {
            animate.clip = clip;
            animate.Play();
        }
    }

    private IEnumerator PlayAnimationCoroutine(AnimationClip clip, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        PlayAnimation(clip);
    }
}
