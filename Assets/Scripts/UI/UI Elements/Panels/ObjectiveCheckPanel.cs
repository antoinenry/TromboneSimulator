using UnityEngine;
using TMPro;
using System.Collections;

[ExecuteAlways]
public class ObjectiveCheckPanel : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text textField;
    public TMP_Text textGlow;
    [Header("Animations")]
    public AnimationClip checkedAnimation;
    public AnimationClip uncheckedAnimation;
    public AnimationClip disappearAnimation;

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

    public void PlayCheckedAnimation() => PlayAnimation(checkedAnimation);

    public void PlayUncheckedAnimation() => PlayAnimation(uncheckedAnimation);

    public void PlayDisappearAnimation(bool destroyOnAnimationEnd)
    {
        if (destroyOnAnimationEnd) StartCoroutine(PlayAnimationThenDestroyCoroutine(disappearAnimation));
        else PlayAnimation(disappearAnimation);
    }

    private void PlayAnimation(AnimationClip clip)
    {
        if (anim == null) return;
        anim.Stop();
        anim.clip = clip;
        if (Application.isPlaying) anim.Play();
    }

    private IEnumerator PlayAnimationThenDestroyCoroutine(AnimationClip clip)
    {
        PlayAnimation(clip);
        if (anim != null) yield return new WaitWhile(() => anim.isPlaying);
        Destroy(gameObject);
    }
}
