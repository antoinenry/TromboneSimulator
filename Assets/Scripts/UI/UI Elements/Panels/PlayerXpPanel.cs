using TMPro;
using UnityEngine;

[ExecuteAlways]
public class PlayerXpPanel : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text xpField;
    public Animation animate;
    [Header("Content")]
    public int xpValue;
    public string xpPrefix = "Niveau ";
    public AnimationClip setXpAnimation;

    private void Update()
    {
        if (Application.isPlaying == false) xpField?.SetText(xpPrefix + xpValue);
    }

    public void SetXp(int value)
    {
        if (xpField != null)
            xpField.SetText(xpPrefix + xpValue);
        if (xpValue != value)
        {
            xpValue = value;
            if (animate != null)
            {
                animate.clip = setXpAnimation;
                animate.Play();
            }
        }
    }
}
