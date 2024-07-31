using UnityEngine;

public class UIToggleSFX : MonoBehaviour
{
    public int soundIndex;

    public void OnToggle(bool value)
    {
        if (value) MenuUI.SFXSource?.PlayToggleOnSFX(soundIndex);
        else MenuUI.SFXSource?.PlayToggleOffSFX(soundIndex);
    }
}
