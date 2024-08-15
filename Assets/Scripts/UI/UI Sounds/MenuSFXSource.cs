public class MenuSFXSource : SFXSource
{
    public MenuSFX sfx;

    protected MenuUI menu;

    protected override void Awake()
    {
        base.Awake();
        menu = GetComponent<MenuUI>();
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

    private void OnShowUI() => PlayOneShot(sfx?.showUI);

    private void OnHideUI() => PlayOneShot(sfx?.hideUI);
}