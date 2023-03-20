using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMeter : MonoBehaviour
{
    public float level;

    [Header("Sensibility")]
    public float levelExpansion;
    public float levelScale;
    public float levelOffset;

    private void Update()
    {
        level = GetRawAudioLevel();
        level = Mathf.Log(level);
        level = levelScale * Mathf.Pow(level + levelOffset, levelExpansion);
    }

    private float GetRawAudioLevel()
    {
        float[] spectrumL = new float[256];
        float[] spectrumR = new float[256];
        AudioListener.GetSpectrumData(spectrumL, 0, FFTWindow.Rectangular);
        AudioListener.GetSpectrumData(spectrumR, 1, FFTWindow.Rectangular);
        float level = 0f;
        foreach (float f in spectrumL) level += f;
        foreach (float f in spectrumR) level += f;
        level /= 512f;
        return level;
    }
}
