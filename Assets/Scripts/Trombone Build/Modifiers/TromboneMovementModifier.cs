using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/Modifiers/Movement")]
public class TromboneMovementModifier : TromboneBuildModifier
{
    public enum Axis { All, SlideOnly, PressureOnly }

    public Axis enabledMovements = Axis.All;
    public float toneOffset = 0f;
    public bool changeSlideLength = false;
    public float slideToneLength = 0f;
    public bool changePressureSteps = false;
    public float[] pressureToneSteps = new float[0];

    public override void ApplyTo(TromboneBuild build)
    {
        base.ApplyTo(build);
        if (build?.tromboneAuto != null)
        {
            TromboneAutoSettings autoSettings = build.tromboneAuto.autoSettings;
            switch (enabledMovements)
            {
                case Axis.PressureOnly:
                    autoSettings.slideControl = TromboneAutoSettings.ControlConditions.Always;
                    break;
                case Axis.SlideOnly:
                    autoSettings.pressureControl = TromboneAutoSettings.ControlConditions.Always;
                    break;
            }
            build.tromboneAuto.autoSettings = autoSettings;
        }
        if (build?.tromboneDisplay != null)
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
            build.tromboneDisplay.minSlideTone = 0f;
            if (changeSlideLength) build.tromboneDisplay.maxSlideTone = slideToneLength;
        }
        if (build?.tromboneCore != null)
        {
            build.tromboneCore.baseTone += toneOffset;
            if (changeSlideLength) build.tromboneCore.slideToneLength = slideToneLength;
            if (changePressureSteps) build.tromboneCore.pressureToneSteps = pressureToneSteps;
        }
    }
}