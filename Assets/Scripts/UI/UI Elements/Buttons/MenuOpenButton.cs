using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuOpenButton : MenuNavigationButton
{
    public MenuUI destination;

    override protected void OnButtonClicked()
    {
        if (origin != null) origin.GoTo(destination);
    }
}