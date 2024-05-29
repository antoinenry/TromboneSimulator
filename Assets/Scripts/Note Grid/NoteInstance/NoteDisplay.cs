using UnityEngine;
using System;
using UnityEngine.UIElements;

[ExecuteAlways]
public class NoteDisplay : MonoBehaviour
{
    [Serializable]
    public struct NoteSprite
    {
        public Sprite single;
        public Sprite linkedToNext;
        public Sprite linkedToPrevious;
        public Sprite linkedToBoth;

        public Sprite Get(LinkType link)
        {
            if (link.HasFlag(LinkType.ToNext))
            {
                if (link.HasFlag(LinkType.ToPrevious)) return linkedToBoth;
                else return linkedToNext;
            }
            else if (link.HasFlag(LinkType.ToPrevious)) return linkedToPrevious;
            else return single;
        }
    }

    [Flags]
    public enum LinkType { Single = 0, ToNext = 1, ToPrevious = 2 }

    [Header("Components")]
    public SpriteCutter hRenderer;
    public SpriteCutter vRenderer;
    public SpriteRenderer target;
    [Header("Sprites")]
    public NoteSprite hSprites;
    public NoteSprite vSprites;
    public LinkType noteLink;
    [Header("Dimensions")]
    public float distance = 0f;
    public float distanceOffset = 0f;
    [Min(0f)] public float length = 16f;
    public bool roundValues = true;
    [Header("Colors")]
    public Color baseColor = Color.green;
    public Color farTint = Color.gray;
    public Color incomingTint = Color.white;
    public Color playedTint = Color.white;
    public Color missedTint = Color.red;
    public Color targetTint = Color.white;
    public float farDistance = 60f;
    public float incomingDistance = 30f;

    private void Start()
    {
        SetLength(length);
        SetDistance(distance);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            SetSprites();
            SetLength(length);
            SetDistance(distance);
        }
    }

    public void SetSprites()
    {
        if (hRenderer != null) hRenderer.spriteRenderer.sprite = hSprites.Get(noteLink);
        if (vRenderer != null) vRenderer.spriteRenderer.sprite = vSprites.Get(noteLink);
    }

    public void SetSprites(bool linkToPreviousNote, bool linkToNextNote)
    {
        noteLink = LinkType.Single;
        //if (linkToPreviousNote) noteLink |= LinkType.ToPrevious;
        //if (linkToNextNote) noteLink |= LinkType.ToNext;
        SetSprites();
    }

    public void SetLength(float l)
    {
        length = Mathf.Max(l, 0f);
        float _length = roundValues ? Mathf.Ceil(length) : length;
        if (hRenderer != null) hRenderer.SetTotalLength(_length);
        if (vRenderer != null) vRenderer.SetTotalLength(_length);
    }

    public void SetDistance(float d)
    {
        // Set distance value
        distance = d;
        float _distance = roundValues ? Mathf.Ceil(distance) : distance;
        // Get incoming progress
        float distanceRatio = Mathf.Clamp01(distance / incomingDistance);
        // Set sprite positions
        if (hRenderer != null) hRenderer.transform.localPosition = (_distance + distanceOffset) * Vector3.right;
        if (vRenderer != null) vRenderer.transform.localPosition = (_distance + distanceOffset) * Vector3.up;
        // Incoming color and target
        if (distance >= 0f)
        {
            if (distance <= incomingDistance)
            {
                // Color
                Color incomingColor = TintColor(baseColor, incomingTint, distanceRatio);
                SetColor(incomingColor, 0f, float.PositiveInfinity);
                // Set target alpha
                if (distanceRatio > 0f && target != null)
                {
                    Color targetColor = TintColor(baseColor, targetTint);
                    targetColor.a *= distanceRatio < 1f ? distanceRatio : 0f;
                    target.color = targetColor;
                }
            }
            else if (distance <= farDistance)
            {
                // Fade color
                float farRatio = 1f - (distance - incomingDistance) / (farDistance - incomingDistance);
                Color fadeColor = TintColor(baseColor, farTint, farRatio);
                SetColor(fadeColor, 0f, float.PositiveInfinity);
                // Hide target
                target.color = Color.clear;
            }
            else
            {
                // Transparent note and target
                SetColor(Color.clear, 0f, float.PositiveInfinity);
                target.color = Color.clear;
            }
        }
    }

    public void Miss(float fromPosition, float toPosition)
    {
        // Set color
        Color missedColor = TintColor(baseColor, missedTint);
        SetColor(missedColor, fromPosition, toPosition);
    }

    public void Play(float fromPosition, float toPosition, float accuracy)
    {
        // Cut played bits at negative positions (past part of note)
        Cut(Mathf.Min(fromPosition, 0f), Mathf.Min(toPosition, 0f));
        // Set played color at positive positions (upcoming part of note)
        Color playColor = TintColor(baseColor, Color.Lerp(missedTint, playedTint, accuracy));
        SetColor(playColor, Mathf.Max(fromPosition, 0f), Mathf.Max(toPosition, 0f));
    }

    public void FullCatch()
    {
        if (target != null)
        {
            Animation anim = target.GetComponent<Animation>();
            if (anim != null) anim.Play();
        }
    }

    public void Cut(float fromPosition, float toPosition)
    {
        CutLocal(fromPosition - distance, toPosition - distance);
    }

    public void CutLocal(float fromPosition, float toPosition)
    {
        // Round values
        if (roundValues)
        {
            fromPosition = Mathf.Ceil(fromPosition);
            toPosition = Mathf.Ceil(toPosition);
        }
        if (hRenderer != null) hRenderer.Cut(fromPosition, toPosition);
        if (vRenderer != null) vRenderer.Cut(fromPosition, toPosition);
    }

    public void SetVisible(bool horizontal, bool vertical)
    {
        if (hRenderer != null) hRenderer.SetVisible(horizontal);
        if (vRenderer != null) vRenderer.SetVisible(vertical);
    }

    public void SetColor(Color c, float fromPosition, float toPosition)
    {
        SetColorLocal(c, fromPosition - distance, toPosition - distance);
    }

    public void SetColorLocal(Color c, float fromPosition, float toPosition)
    {
        if (roundValues)
        {
            fromPosition = Mathf.Ceil(fromPosition);
            toPosition = Mathf.Ceil(toPosition);
        }
        if (hRenderer != null) hRenderer.SetColorFromToPosition(c, fromPosition, toPosition, false, true);
        if (vRenderer != null) vRenderer.SetColorFromToPosition(c, fromPosition, toPosition, false, true);
    }

    private Color TintColor(Color c, Color tint, float blend = 1f)
    {
        float alpha = c.a;
        float ratio = blend * tint.a;
        Color tintColor = (1f - ratio) * c + ratio * tint;
        tintColor.a = alpha;
        return tintColor;
    }
}
