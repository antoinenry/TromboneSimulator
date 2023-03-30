using System;
using UnityEngine;

static public class AudioSampling
{
    static public float[] GetSamples(AudioClip audio)
    {
        float[] samples;
        if (audio != null)
        {
            samples = new float[audio.samples];
            audio.GetData(samples, 0);
        }
        else samples = null;
        return samples;
    }

    static public float[] GetMonoSamples(AudioClip audio)
    {
        float[] samples = GetSamples(audio);
        if (samples != null && audio.channels == 2) return StereoToMono(samples);
        return samples;
    }

    static public float[] GetStereoSamples(AudioClip audio, float pan = 0f)
    {
        float[] samples = GetSamples(audio);
        if (samples != null && audio.channels == 1) return MonoToStereo(samples, pan);
        return samples;
    }

    static public void AddTo(ref float[] destination, float[] samples, int sampleOffset)
    {
        if (samples != null)
        {
            int sampleLength = samples.Length;
            if (destination != null)
            {
                for (int s = 0; s < sampleLength; s++)
                {
                    if (sampleOffset + s >= destination.Length) break;
                    if (sampleOffset + s >= 0) destination[sampleOffset + s] += samples[s];
                }
            }
        }
    }

    static public void Normalize(ref float[] samples)
    {
        if (samples != null)
        {
            float peakLevel = 0f;
            foreach (float s in samples) peakLevel = Mathf.Max(peakLevel, Mathf.Abs(s));
            float normalizeGain = peakLevel > 0f ? 1f / peakLevel : 1f;
            samples = Array.ConvertAll(samples, s => s * normalizeGain);
        }
    }

    static public void FadeIn(ref float[] samples, int channels, int fadeInSamples, int startSamples = 0)
    {
        if (samples != null && channels != 0)
        {
            int sampleCount = samples.Length / channels;
            float fade;
            for (int s = 0; s < fadeInSamples && s < sampleCount; s++)
            {
                fade = (float)(s + 1) / fadeInSamples;
                for (int c = 0; c < channels; c++)
                    samples[(startSamples + s) * channels + c] *= fade;
            }
        }
    }

    static public void FadeOut(ref float[] samples, int channels, int fadeOutSamples, int endSamples)
    {
        if (samples != null && channels != 0)
        {
            int sampleCount = samples.Length / channels;
            float fade;
            for (int s = 0; s < fadeOutSamples && s < sampleCount; s++)
            {
                fade = (float)(s + 1) / fadeOutSamples;
                for (int c = 0; c < channels; c++)
                    samples[(endSamples - s) * channels + c] *= fade;
            }
        }
    }

    static public void FadeOut(ref float[] samples, int channels, int fadeOutSamples)
    {
        if (samples != null && channels != 0)
        {
            int sampleCount = samples.Length / channels;
            FadeOut(ref samples, channels, fadeOutSamples, sampleCount - 1);
        }
    }

    static public float[] MonoToStereo(float[] monoSamples, float pan = 0f)
    {
        if (monoSamples != null)
        {
            float rightGain = (1f + Mathf.Clamp(pan, -1f, 1f)) / 2f;
            float leftGain = 1f - rightGain;
            int samplesCount = monoSamples.Length;
            float[] stereoSamples = new float[samplesCount * 2];
            for (int s = 0; s < samplesCount; s++)
            {
                stereoSamples[s * 2] = monoSamples[s] * leftGain;
                stereoSamples[s * 2 + 1] = monoSamples[s] * rightGain;
            }
            return stereoSamples;
        }
        else
            return null;
    }

    static public float[] StereoToMono(float[] stereoSamples)
    {
        if (stereoSamples != null)
        {
            int sampleCount = stereoSamples.Length / 2;
            float[] monoSamples = new float[sampleCount];
            for (int s = 0; s < sampleCount; s++)
            {
                monoSamples[s] = (stereoSamples[s * 2] + stereoSamples[s * 2 + 1]) / 2f;
            }
            return monoSamples;
        }
        else
            return null;
    }

    static public float[] Pitch(float[] samples, int channels, float audioPitch)
    {
        if (samples != null)
        {
            int sampleCount = samples.Length / channels;
            int pitchedSampleCount = Mathf.CeilToInt(sampleCount / audioPitch);
            float[] pitchedSamples = new float[pitchedSampleCount * channels];
            for (int s = 0; s < pitchedSampleCount; s++)
            {
                int floor = Mathf.FloorToInt(s * audioPitch);
                for (int c = 0; c < channels; c++)
                {
                    if (floor + 1 < sampleCount - 1)
                        pitchedSamples[s * channels + c] = Mathf.Lerp(samples[floor * channels + c], samples[(floor + 1) * channels + c], s / audioPitch - floor);
                    else
                        pitchedSamples[s * channels + c] = samples[floor * channels + c];
                }
            }
            return pitchedSamples;
        }
        else
            return null;
    }
}
