using UnityEngine;

[ExecuteAlways]
public class MainMenu : MenuUI
{
    public GameObject menuButtonsContainer;

    public override void ShowUI()
    {
        base.ShowUI();
        onLoadingScreenVisible?.AddListener(OnLoadingScreenVisible);
    }

    public override void HideUI()
    {
        base.HideUI();
        onLoadingScreenVisible?.RemoveListener(OnLoadingScreenVisible);
    }

    private void OnLoadingScreenVisible(LoadingScreen loadingScreen, bool isLoadingVisible)
    {
        if (loadingScreen != null && isLoadingVisible) loadingScreen.showAsPanel = true;
        if (menuButtonsContainer) menuButtonsContainer.SetActive(!isLoadingVisible);
    }
}