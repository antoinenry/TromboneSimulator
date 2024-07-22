using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuStartGameButton : MenuNavigationButton
{
    override protected void OnButtonClicked()
    {
        origin?.HideUI();
        MenuUI.onStartLevel.Invoke();
    }
}