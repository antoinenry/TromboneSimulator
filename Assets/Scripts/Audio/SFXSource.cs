using UnityEngine;
using System;
using System.Collections;

public class SFXSource : MonoBehaviour
{
    protected AudioSource source;

    protected virtual void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayLoop(AudioClip clip, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(DelayedActionCoroutine(() => PlayLoop(clip), delay));
            return;
        }
        if (source == null || clip == null) return;
        if (source.clip == clip && source.loop == true && source.isPlaying == true) return;
        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    public void StopLoop(AudioClip clip, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(DelayedActionCoroutine(() => StopLoop(clip), delay));
            return;
        }
        if (clip != null && source != null && source.isPlaying && source.clip == clip) source.Stop();
    }

    public void PlayOneShot(AudioClip clip, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(DelayedActionCoroutine(() => PlayOneShot(clip), delay));
            return;
        }
        if (source != null && clip != null) source.PlayOneShot(clip);
    }

    public void PlayRandomOneShot(AudioClip[] clips, float delay = 0f)
    {
        int clipCount = clips != null ? clips.Length : 0;
        if (clipCount > 0) PlayOneShot(clips[UnityEngine.Random.Range(0, clipCount)], delay);
    }

    private IEnumerator DelayedActionCoroutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}
