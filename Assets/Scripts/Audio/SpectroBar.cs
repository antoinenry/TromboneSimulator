using UnityEngine;
using UnityEngine.UI;

public class SpectroBar : MonoBehaviour
{
    public AudioMeter audioMeter;

    [Header("Look")]
    //public SpriteRenderer barPrefab;
    //[Min(0)] public int width = 6;
    //[Range(0f, 1f)] public float spacing = 0f;
    public float minHeight = 1f;
    public float maxHeight = 10f;
    [Header("Resolution")]
    [Min(0f)] public int bufferSize;
    [Min(0f)] public float sampleRate;
    [Min(0f)] public float vSmoothing;

    private Image[] bars;
    private float[] levelBuffer;
    private float scrollTimer;
    private float[] barHeights;
    private int barCount;

    private void Awake()
    {
        bars = GetComponentsInChildren<Image>();
        barCount = bars.Length;
    }

    void Update()
    {
        ScrollLevels();
    }

    private void ScrollLevels()
    {
        if (bufferSize <= 0) bufferSize = 1;
        if (levelBuffer == null || levelBuffer.Length != bufferSize) levelBuffer = new float[bufferSize];
        if (barHeights == null || barHeights.Length != barCount) barHeights = new float[barCount];

        float deltaTime = Time.deltaTime;
        scrollTimer += deltaTime * sampleRate;

        if (scrollTimer > 1f)
        {
            scrollTimer -= 1f;

            levelBuffer[bufferSize-1] = (audioMeter != null) ? audioMeter.level : 0f;
            for (int i = 0; i < bufferSize - 1; i++)
                levelBuffer[i] = levelBuffer[i + 1];

            int delay = Mathf.FloorToInt(bufferSize / barCount);
            for (int i = 0; i < barCount; i++)
                barHeights[i] = Mathf.Clamp(levelBuffer[i * delay], minHeight, maxHeight);
        }
        else
        {
            if (vSmoothing > 0f)
            {
                for (int i = 0; i < barCount; i++)
                {
                    Vector2 smoothedBarSize = bars[i].rectTransform.sizeDelta;
                    smoothedBarSize.y = Mathf.MoveTowards(smoothedBarSize.y, barHeights[i], deltaTime / vSmoothing);
                    bars[i].rectTransform.sizeDelta = smoothedBarSize;
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    Vector2 barSize = bars[i].rectTransform.sizeDelta;
                    barSize.y = barHeights[i];
                    bars[i].rectTransform.sizeDelta = barSize;
                }
            }
        }
    }
}
