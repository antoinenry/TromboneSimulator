using UnityEngine;

public class MenuSFXSource : MonoBehaviour
{
    public AudioSource source;

    public void Play(AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip);
    }
}
