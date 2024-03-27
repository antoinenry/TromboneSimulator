using UnityEngine;

[CreateAssetMenu(fileName = "NewTrombone", menuName = "Trombone Hero/Instruments/Trombone Build")]
public class TromboneBuild : ScriptableObject
{
    [Header("Tones")]
    public float slideTones;
    public float[] pressureStepTones;
    [Header("Aspect")]
    public Color color = Color.white;
    public int bodyLength = 128;
    public int slideLength = 128;
    [Header("Controls")]
    public bool horizontalMovements = true;
    public bool verticalMovements = true;
    //public List<TromboneControlWiring.Wire> controlWiring;
    public TromboneAutoSettings autoSettings;

    public void LoadTo(TromboneCore trombone)
    {
        // Copy settings to trombone components
        if (trombone != null)
        {
            // Trombone component
            trombone.pressureStepTones = pressureStepTones;
            trombone.slideTones = slideTones;
            // TromboneDisplay component
            TromboneDisplay visual = trombone.tromboneDisplay;
            if (visual != null)
            {
                // Aspect
                visual.color = color;
                visual.bodyLength = bodyLength;
                visual.slideLength = slideLength;
                visual.UpdateAspect();
                // Controls
                visual.enableSlideMovement = horizontalMovements;
                visual.enablePressureMovement = verticalMovements;
            }
            // TromboneAuto component
            TromboneAuto auto = trombone.tromboneAuto;
            if (auto != null)
            {
                // Auto settings
                auto.settings = autoSettings;
            }
            // Signal changes
            trombone.onChangeBuild.Invoke();
        }
    }

    public void SaveFrom(TromboneCore trombone)
    {
        // Copy settings from trombone components
        if (trombone != null)
        {
            // Trombone component
            pressureStepTones = trombone.pressureStepTones;
            slideTones = trombone.slideTones;
            // TromboneDisplay component
            TromboneDisplay visual = trombone.tromboneDisplay;
            if (visual != null)
            {
                // Aspect
                color = visual.color;
                bodyLength = visual.bodyLength;
                slideLength = visual.slideLength;
                // Controls
                horizontalMovements = visual.enableSlideMovement;
                verticalMovements = visual.enablePressureMovement;
            }
            // TromboneAuto component
            TromboneAuto auto = trombone.tromboneAuto;
            if (auto != null)
            {
                // Auto settings
                autoSettings = auto.settings;
            }
        }
    }

    public void CopyFrom(TromboneBuild other)
    {
        // Copy settings from other build
        if (other != null)
        {
            // Core
            pressureStepTones = other.pressureStepTones;
            slideTones = other.slideTones;
            // Aspect
            color = other.color;
            bodyLength = other.bodyLength;
            slideLength = other.slideLength;
            // Controls
            horizontalMovements = other.horizontalMovements;
            verticalMovements = other.verticalMovements;
            // Auto settings
            autoSettings = other.autoSettings;
        }
    }
}
