using UnityEngine;

public class NoteTarget : MonoBehaviour
{
    public enum DistanceState { Far, Incoming, Next, Current, Passed };

    public Sprite incomingSprite;
    public Sprite nextNoteSprite;
    public Sprite currentNoteSprite;
    public Sprite passedNoteSprite;
    public float farAlpha = .5f;
    public float closeAlpha = 1.0f;
    public bool fullCatch = false;

    private SpriteRenderer spriteRenderer;
    private Color color = Color.white;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color c)
    {
        color = c;
        if (spriteRenderer) spriteRenderer.color = new(c.r, c.g, c.b, spriteRenderer.color.a);
    }

    public void SetDistanceRatio(float d)
    {
        if (spriteRenderer == null || fullCatch) return;
        float a = Mathf.Lerp(farAlpha, closeAlpha, d);
        spriteRenderer.color = new Color(color.r, color.g, color.b, a);
    }

    public void SetSprite(DistanceState distance)
    {
        if (spriteRenderer == null) return;
        switch (distance)
        {
            case DistanceState.Far:
                spriteRenderer.enabled = false;
                break;
            case DistanceState.Incoming:
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = incomingSprite;
                break;
            case DistanceState.Next:
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = nextNoteSprite;
                break;
            case DistanceState.Current:
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = currentNoteSprite;
                break;
            case DistanceState.Passed:
                spriteRenderer.sprite = passedNoteSprite;
                spriteRenderer.enabled = fullCatch;
                break;
        }
    }

    public void PlayCatchAnimation()
    {
        fullCatch = true;
        Animation anim = GetComponent<Animation>();
        if (anim != null) anim.Play();
    }
}