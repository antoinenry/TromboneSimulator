public class MenuSFXSource : SFXSource
{
    public MenuSFX sfx;

    protected MenuUI menu;

    protected override void Awake()
    {
        base.Awake();
        menu = GetComponent<MenuUI>();
    }

    private void Start()
    {
        if (AudioSourceComponent == null) AudioSourceComponent = MenuUI.SFXSource.AudioSourceComponent;
    }

    private void OnEnable()
    {
        menu?.onShowUI?.AddListener(OnShowUI);
        menu?.onHideUI?.AddListener(OnHideUI);
    }

    private void OnDisable()
    {
        menu?.onShowUI?.RemoveListener(OnShowUI);
        menu?.onHideUI?.RemoveListener(OnHideUI);
    }

    private void OnShowUI()
    {
        if (sfx == null) return;
        PlayOneShot(sfx.showUI);
        PlayLoop(sfx.visibleLoop, sfx.loopDelay);
    }

    private void OnHideUI()
    {
        if (sfx == null) return;
        PlayOneShot(sfx.hideUI);
        StopLoop(sfx.visibleLoop);
    }
}