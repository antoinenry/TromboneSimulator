using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Instruments/Trombone Build/Modifiers")]
public class MovementModifier : TromboneBuildModifier
{
    public override bool CanStack => false;

    public enum Axis { All, SlideOnly, PressureOnly }

    public Axis enabledMovements = Axis.All;
    [Tone] public float toneOffset = 0f;
    public float slideToneLength;
    public float[] pressureToneSteps;

    public override void ApplyTo(TromboneCustomizer build)
    {
        if (build == null || build.tromboneCore == null || build.tromboneDisplay == null ) return;
        switch(enabledMovements)
        {
            case Axis.All:
                build.tromboneDisplay.enablePressureMovement = true;
                build.tromboneDisplay.enableSlideMovement = true;
                break;
            case Axis.PressureOnly:
                build.tromboneDisplay.enablePressureMovement = true;
                build.tromboneDisplay.enableSlideMovement = false;
                break;
            case Axis.SlideOnly:
                build.tromboneDisplay.enablePressureMovement = false;
                build.tromboneDisplay.enableSlideMovement = true;
                break;
        }
        build.tromboneCore.baseTone += toneOffset;
        build.tromboneCore.slideToneLength = slideToneLength;
        build.tromboneCore.pressureToneSteps = pressureToneSteps;
        build.tromboneDisplay.minSlideTone = 0f;
        build.tromboneDisplay.maxSlideTone = slideToneLength;
    }
}