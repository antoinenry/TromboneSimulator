using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/UI SFX/Counter Display")]
public class CounterDisplaySFX : ScriptableObject
{
    public AudioClip starting;
    public AudioClip stopping;
    public AudioClip moving;
    public float rate = 10f;
    public float startPitch = 1f;
    public float pitchRiseSpeed = .1f;
    public float maxPitch = 1.5f;
    public float stopDelay = .5f;
}