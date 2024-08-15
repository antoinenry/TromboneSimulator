using UnityEngine;

[CreateAssetMenu(menuName = "Trombone Hero/UI SFX/Menu")]
public class MenuSFX : ScriptableObject
{
    public AudioClip showUI;
    public AudioClip visibleLoop;
    public float loopDelay;
    public AudioClip hideUI;
}