using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuCloseButton : MenuNavigationButton
{
    override protected void OnButtonClicked()
    {
        if (origin != null)
        {
            if (origin.PreviousUI != null) origin.GoBack();
            else
            {
                if (Application.isEditor)
                {
                    Debug.Log("Application Quit");
                    Debug.Break();
                }
                else Application.Quit();
            }
        }
    }
}