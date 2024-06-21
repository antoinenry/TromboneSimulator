using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Movement")]
public class TromboneMovementModifier : TromboneBuildModifier
{
    public enum Axis { All, SlideOnly, PressureOnly }

    public Axis enabledMovements = Axis.All;
    public float toneOffset = 0f;
    public float slideToneLength;
    public float[] pressureToneSteps;

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.tromboneCore != null)
        {
            switch (enabledMovements)
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
        }
        if (build?.tromboneDisplay != null)
        {
            build.tromboneDisplay.minSlideTone = 0f;
            build.tromboneDisplay.maxSlideTone = slideToneLength;
        }
    }
}