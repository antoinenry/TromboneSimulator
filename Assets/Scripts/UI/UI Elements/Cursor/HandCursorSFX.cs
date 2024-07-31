using UnityEngine;
using System;
using static HandCursor;

public class HandCursorSFX : MonoBehaviour
{
    [Serializable]
    public struct SFXTrigger
    {
        public AudioClip clip;
        public CursorState cursorState;
        public bool invertCondition;
    }

    public SFXTrigger[] triggers;

    private CursorState cursorState;
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void OnCursorStateChange(CursorState stateChange)
    {
        if (source == null || triggers == null || cursorState == stateChange) return;
        foreach (SFXTrigger trigger in triggers)
        {
            if (trigger.clip == null) continue;
            if (trigger.invertCondition)
            {
                if (cursorState.HasFlag(trigger.cursorState) && !stateChange.HasFlag(trigger.cursorState))
                    source.PlayOneShot(trigger.clip);
            }
            else
            {
                if (!cursorState.HasFlag(trigger.cursorState) && stateChange.HasFlag(trigger.cursorState))
                    source.PlayOneShot(trigger.clip);
            }
        }
        cursorState = stateChange;
    }
}
