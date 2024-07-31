using UnityEngine;

public class BooleanAudioClip : MonoBehaviour
{
    public AudioClip trueClip;
    public AudioClip falseClip;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayBool(bool value)
    {
        if (source == null) return;
        source.PlayOneShot(value ? trueClip : falseClip);
    }
}
