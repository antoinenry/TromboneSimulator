using UnityEngine;

public class UISoundFXSource : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] buttonClicks;
    public AudioClip[] buttonHovers;
    public AudioClip[] toggleOns;
    public AudioClip[] toggleOffs;

    private void PlayAt(AudioClip[] clips, int index)
    {
        if (source == null) return;
        int sfxCount = clips != null ? clips.Length : 0;
        if (sfxCount == 0) return;
        index = Mathf.Clamp(index, 0, sfxCount - 1);
        source.PlayOneShot(clips[index]);
    }

    public void PlayButtonClickSFX(int index = 0) => PlayAt(buttonClicks, index);
    public void PlayButtonHoverSFX(int index = 0) => PlayAt(buttonHovers, index);
    public void PlayToggleOnSFX(int index = 0) => PlayAt(toggleOns, index);
    public void PlayToggleOffSFX(int index = 0) => PlayAt(toggleOffs, index);
}
