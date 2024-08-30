using UnityEngine;
using System.Collections;

public class CounterDisplaySFXSource : MonoBehaviour
{
    public AudioSource audioSource;
    public CounterDisplay counter;
    public CounterDisplaySFX sfx;

    public Coroutine movingCoroutine;

    private void OnDisable() => Stop();

    private void Update()
    {
        if (counter == null) return;
        if (counter.IsMoving && movingCoroutine == null) movingCoroutine = StartCoroutine(SFXCoroutine());
    }

    private IEnumerator SFXCoroutine()
    {
        if (audioSource == null || counter == null || sfx == null)
        {
            movingCoroutine = null;
            yield break;
        }
        // Starting sequence
        if (sfx.starting != null)
        {
            audioSource.Stop();
            audioSource.pitch = 1f;
            audioSource.clip = sfx.starting;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        // Moving sequence
        if (sfx.moving != null)
        {
            float stopTimer = 0f;
            audioSource.clip = sfx.moving;
            audioSource.pitch = sfx.startPitch;
            audioSource.Play();
            if (sfx.rate > 0f)
            {
                float spacing = 1f / sfx.rate;
                while (audioSource != null && counter != null)
                {
                    yield return new WaitForSeconds(spacing);
                    audioSource.Stop();
                    audioSource.pitch += sfx.pitchRiseSpeed * spacing;
                    audioSource.Play();
                    if (counter.IsMoving == false) stopTimer += spacing;
                    else stopTimer = 0f;
                    if (stopTimer >= sfx.stopDelay) break;
                }
            }
        }
        // Stopping sequence
        if (sfx.stopping != null)
        {
            audioSource.Stop();
            audioSource.clip = sfx.stopping;
            audioSource.pitch = 1f;
            audioSource.Play();
        }
        // End
        movingCoroutine = null;
    }

    public void Stop()
    {
        StopAllCoroutines();
        movingCoroutine = null;
    }
}