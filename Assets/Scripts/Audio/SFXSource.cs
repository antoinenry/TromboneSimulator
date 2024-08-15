using UnityEngine;

public class SFXSource : MonoBehaviour
{
    protected AudioSource source;

    protected virtual void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayLoop(AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    public void StopLoop(AudioClip clip)
    {
        if (clip != null && source != null && source.isPlaying && source.clip == clip) source.Stop();
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (source != null && clip != null) source.PlayOneShot(clip);
    }

    public void PlayRandomOneShot(AudioClip[] clips)
    {
        int clipCount = clips != null ? clips.Length : 0;
        if (clipCount > 0) PlayOneShot(clips[Random.Range(0, clipCount)]);
    }
}
