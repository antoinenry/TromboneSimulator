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
        if (CheckTimeSpacing() == false) return;
        if (random) sfxIndex = PlayRandomOneShot(sfxClips);
        else
        {
            int sfxCount = sfxClips != null ? sfxClips.Length : 0;
            if (sfxCount == 0)
            {
                sfxIndex = -1;
                return;
            }
            if (sfxCount == 1)
            {
                sfxIndex = 0;
            }
            else
            {
                sfxIndex = (int)Mathf.Repeat(sfxIndex + 1, sfxCount - 1);
            }                
            PlayOneShot(sfxClips[sfxIndex]);
        }
    }

    public void PlaySFX(int index)
    {
        if (CheckTimeSpacing() == false) return;
        int sfxCount = sfxClips != null ? sfxClips.Length : 0;
        if (index < 0 || index >= sfxCount) return;
        sfxIndex = index;
        PlayOneShot(sfxClips[sfxIndex]);
    }

    private bool CheckTimeSpacing()
    {
        float time = Time.time;
        if (time < lastPlayTime + spacing) return false;
        lastPlayTime = time;
        return true;
    }
}