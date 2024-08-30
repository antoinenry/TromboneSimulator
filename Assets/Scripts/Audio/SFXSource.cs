using UnityEngine;
using System;
using System.Collections;

public class SFXSource : MonoBehaviour
{
    public AudioSource AudioSourceComponent { get; protected set; }

    protected virtual void Awake()
    {
        AudioSourceComponent = GetComponent<AudioSource>();
    }

    public void PlayLoop(AudioClip clip, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(DelayedActionCoroutine(() => PlayLoop(clip), delay));
            return;
        }
        if (AudioSourceComponent == null || clip == null) return;
        if (AudioSourceComponent.clip == clip && AudioSourceComponent.loop == true && AudioSourceComponent.isPlaying == true) return;
        AudioSourceComponent.clip = clip;
        AudioSourceComponent.loop = true;
        AudioSourceComponent.Play();
    }

    public void StopLoop(AudioClip clip, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(DelayedActionCoroutine(() => StopLoop(clip), delay));
            return;
        }
        if (clip != null && AudioSourceComponent != null && AudioSourceComponent.isPlaying && AudioSourceComponent.clip == clip) AudioSourceComponent.Stop();
    }

    public void PlayOneShot(AudioClip clip, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(DelayedActionCoroutine(() => PlayOneShot(clip), delay));
            return;
        }
        if (AudioSourceComponent != null && clip != null) AudioSourceComponent.PlayOneShot(clip);
    }

    public int PlayRandomOneShot(AudioClip[] clips, float delay = 0f)
    {
        int clipCount = clips != null ? clips.Length : 0;
        if (clipCount == 0) return -1;
        int randomIndex = UnityEngine.Random.Range(0, clipCount);
        if (clipCount > 0) PlayOneShot(clips[randomIndex], delay);
        return randomIndex;
    }

    private IEnumerator DelayedActionCoroutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}
