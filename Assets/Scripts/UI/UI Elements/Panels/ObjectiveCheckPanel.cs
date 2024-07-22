using UnityEngine;
using TMPro;

[ExecuteAlways]
public class ObjectiveCheckPanel : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text textField;
    public TMP_Text textGlow;
    [Header("Animations")]
    public AnimationClip checkedAnimation;
    public AnimationClip uncheckedAnimation;

    private Animation anim;

    private void Awake()
    {
        anim = GetComponent<Animation>();
    }

    public void SetText(string value)
    {
        if (textField) textField.text = value;
        if (textGlow) textGlow.text = value;
    }

    public void PlayCheckedAnimation()
    {
        if (anim == null) return;
        anim.Stop();
        anim.clip = checkedAnimation;
        if (Application.isPlaying) anim.Play();
    }

    public void PlayUncheckedAnimation()
    {
        if (anim == null) return;
        anim.Stop();
        anim.clip = uncheckedAnimation;
        if (Application.isPlaying) anim.Play();
    }
}
