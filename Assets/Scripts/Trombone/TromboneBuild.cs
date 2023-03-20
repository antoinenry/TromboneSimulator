using UnityEngine;

[CreateAssetMenu(fileName = "NewTrombone", menuName = "Trombone Hero/Instruments/Trombone Build")]
public class TromboneBuild : ScriptableObject
{
    [Header("Tones")]
    public SamplerInstrument instrument;
    public float slideTones;
    public float[] pressureStepTones;
    [Header("Aspect")]
    public Color color = Color.white;
    public int bodyLength = 128;
    public int slideLength = 128;
    [Header("Auto")]
    public TromboneAutoSettings autoSettings;
    public float correctLockToneTolerance = 1f;

    public void Load(Trombone trombone)
    {
        if (trombone != null)
        {
            // Trombone sound
            //trombone.audioInstrument = instrument;
            trombone.pressureStepTones = pressureStepTones;
            trombone.slideTones = slideTones;
            // Trombone aspect
            TromboneDisplay visual = trombone.tromboneDisplay;
            if (visual != null)
            {
                visual.color = color;
                visual.bodyLength = bodyLength;
                visual.slideLength = slideLength;
                visual.UpdateAspect();
            }
            // Auto setting
            //trombone.autoSettings.mode = autoMode;
        }
    }

    public void Save(Trombone trombone)
    {
        // Trombone sound
        if (trombone != null)
        {
            //instrument = trombone.audioInstrument;
            slideTones = trombone.slideTones;
            pressureStepTones = trombone.pressureStepTones;
            // Trombone aspect
            TromboneDisplay visual = trombone.tromboneDisplay;
            if (visual != null)
            {
                color = visual.color;
                bodyLength = visual.bodyLength;
                slideLength = visual.slideLength;
            }
            // Auto settings
            //autoMode = trombone.autoSettings.mode;
        }
    }
}
