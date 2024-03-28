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
    public TromboneAutoSettings autoSettings;
    //public List<TromboneControlWiring.Wire> controlWiring;
    [Header("Level Modifiers")]
    public float tempoStrecher = 1f;
    public float timeStrecher = 1f;
    public float scoreMultiplier = 1f;
    public float maxHealth = 1f;

    public void CopyFrom(TromboneBuild other)
    {
        // Copy settings from other build
        if (other != null)
        {
            name = other.name + " (Clone)";
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
            // Music modifiers
            tempoStrecher = other.tempoStrecher;
            timeStrecher = other.timeStrecher;
            scoreMultiplier = other.scoreMultiplier;
            maxHealth = other.maxHealth;
        }
    }

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
}
