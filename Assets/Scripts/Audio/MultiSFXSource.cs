using UnityEngine;

public class MultiSFXSource : SFXSource
{
    public AudioClip[] sfxClips;
    public bool random;
    public float spacing = 0f;

    private int sfxIndex;
    private float lastPlayTime;

    protected override void Awake()
    {
        base.Awake();
        lastPlayTime = Time.time;
    }

    public void PlaySFX()
    {
        float time = Time.time;
        if (time < lastPlayTime + spacing) return;
        else lastPlayTime = time;
        if (random) sfxIndex = PlayRandomOneShot(sfxClips);
        else
        {
            int sfxCount = sfxClips != null ? sfxClips.Length : 0;
            if (sfxCount == 0)
            {
                sfxIndex = -1;
                return;
            }
            sfxIndex = (int)Mathf.Repeat(sfxIndex + 1, sfxCount - 1);
            PlayOneShot(sfxClips[sfxIndex]);
        }
    }
}