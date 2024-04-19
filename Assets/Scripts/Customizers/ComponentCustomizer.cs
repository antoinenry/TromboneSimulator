using System;
using UnityEngine;

public interface ComponentCustomizer
{
    public Type GetComponentType();
}

[Serializable]
public class TromboneCoreCustomizer : ComponentCustomizer
{
    public Type GetComponentType() => typeof(TromboneCore);

    [Tone] public float baseTone;
    public float[] pressureToneSteps;
    public float slideToneLength;
}

[Serializable]
public class TromboneDisplayCustomizer : ComponentCustomizer
{
    public Type GetComponentType() => typeof(TromboneDisplay);

    public Color color = Color.white;
    public int bodyLength = 110;
    public int slideLength = 90;
    public bool enableSlideMovement = true;
    public bool enablePressureMovement = true;
}

[Serializable]
public class TromboneAudioCustomizer : ComponentCustomizer
{
    public Type GetComponentType() => typeof(TromboneAudio);

    public SamplerInstrument sampler;
    public float pitch = 1f;
}

[Serializable]
public class TromboneAutoCustomizer : ComponentCustomizer
{
    public Type GetComponentType() => typeof(TromboneAuto);

    public TromboneAutoSettings autoSettings;
}

[Serializable]
public class MusicPlayerCustomizer : ComponentCustomizer
{
    public Type GetComponentType() => typeof(MusicPlayer);
}

[Serializable]
public class PerformanceJudgeCustomizer : ComponentCustomizer
{
    public Type GetComponentType() => typeof(PerformanceJudge);

    public float maxHealth = 1f;
}
