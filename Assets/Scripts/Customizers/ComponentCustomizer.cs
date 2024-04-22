using System;
using UnityEngine;

public interface ComponentCustomizer
{
    public Type GetComponentType();
    public void OnApplyToComponent(UnityEngine.Object component)
    { 
    }
}

[Serializable]
public class TromboneCoreCustomizer : ComponentCustomizer
{
    [Tone] public float baseTone;
    public float[] pressureToneSteps;
    public float slideToneLength;

    public Type GetComponentType() => typeof(TromboneCore);
}

[Serializable]
public class TromboneDisplayCustomizer : ComponentCustomizer
{
    public Color color = Color.white;
    public int bodyLength = 110;
    public int slideLength = 90;
    public bool enableSlideMovement = true;
    public bool enablePressureMovement = true;

    public Type GetComponentType() => typeof(TromboneDisplay);
}

[Serializable]
public class TromboneAudioCustomizer : ComponentCustomizer
{
    public SamplerInstrument sampler;
    public float pitch = 1f;

    public Type GetComponentType() => typeof(TromboneAudio);
}

[Serializable]
public class TromboneAutoCustomizer : ComponentCustomizer
{
    public TromboneAutoSettings autoSettings;

    public Type GetComponentType() => typeof(TromboneAuto);
}

[Serializable]
public class MusicPlayerCustomizer : ComponentCustomizer
{
    public float tempoModifier = 1f;
    public float keyModifier = 0f;
    public float playingSpeed = 1f;

    public Type GetComponentType() => typeof(MusicPlayer);

    public void OnApplyToComponent(UnityEngine.Object component)
    {
        MusicPlayer musicPlayer = component as MusicPlayer;
        if (musicPlayer.NeedsReload()) musicPlayer.LoadMusic();
    }
}

[Serializable]
public class PerformanceJudgeCustomizer : ComponentCustomizer
{
    public float maxHealth = 1f;

    public Type GetComponentType() => typeof(PerformanceJudge);
}
