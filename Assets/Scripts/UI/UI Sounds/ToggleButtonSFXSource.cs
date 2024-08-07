public class ToggleButtonSFXSource : ButtonSFXSource
{
    private ToggleButton toggle;

    protected override void Awake()
    {
        toggle = GetComponent<ToggleButton>();
        button = toggle?.button;
    }

    private void OnEnable()
    {
        toggle?.onToggle.AddListener(OnToggle);
    }

    private void OnDisable()
    {
        toggle?.onToggle.RemoveListener(OnToggle);
    }

    private void OnToggle(bool value)
    {
        if (sfx == null || sfx is ToggleButtonSFX == false) return;
        ToggleButtonSFX toggleSfx = (ToggleButtonSFX)sfx;
        MenuUI.SFXSource?.Play(value ? toggleSfx.toggleOn : toggleSfx.toggleOff);
    }

    protected override bool HasMatchingSFX => sfx != null && sfx is ToggleButtonSFX;
}