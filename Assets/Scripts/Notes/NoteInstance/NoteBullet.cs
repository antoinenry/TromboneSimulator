using UnityEngine;

[ExecuteAlways]
public class NoteBullet : MonoBehaviour
{
    public enum LinkDirection { NoLink = 0, Left = -1, Right = 1 }

    [Header("Components")]
    public SpriteCutter hRenderer;
    public SpriteCutter vRenderer;
    public SpriteRenderer topLinkRenderer;
    public SpriteRenderer bottomLinkRenderer;
    public NoteTarget target;
    [Header("Position")]
    public float distance = 0f;
    public float distanceOffset = 0f;
    [Min(0f)] public float length = 16f;
    public bool isNext = false;
    public bool roundValues = true;
    [Header("Links")]
    public Vector2 linkMaxDistance = new(1f, 1f);
    public LinkDirection topLink;
    public float topLinkLength = 10f;
    public LinkDirection bottomLink;
    public float bottomLinkLength = 10f;
    [Header("Colors")]
    public Color baseColor = Color.green;
    public Color farTint = Color.gray;
    public Color incomingTint = Color.white;
    public Color playedTint = Color.white;
    public Color missedTint = Color.red;
    public float farDistance = 60f;
    public float incomingDistance = 30f;

    private bool bottomCut;
    private bool topCut;
    private float width;
    private Color currentColor;

    private void Start()
    {
        SetLength(length);
        SetDistance(distance);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            SetLength(length);
            SetDistance(distance);
            SetColor(baseColor);
        }
        SetLinkSprites();
    }

    private float Round(float value) => Mathf.Ceil(value);
    public float DisplayLength => roundValues ? Round(length) : length;
    public float DisplayDistance => roundValues ? Round(distance + distanceOffset) : distance + distanceOffset;

    public void SetLength(float value)
    {
        length = Mathf.Max(value, 0f);
        float l = DisplayLength;
        Vector2 linkPosition = Vector2.zero;
        if (hRenderer != null) hRenderer.SetTotalLength(l);
        if (vRenderer != null)
        {
            vRenderer.SetTotalLength(l);
            if (vRenderer.spriteRenderer != null) width = vRenderer.spriteRenderer.size.x;
        }
        if (topLinkRenderer != null)
        {
            topLinkRenderer.size = new(topLinkLength, topLinkRenderer.size.y);
            linkPosition.x = topLink == LinkDirection.Left ? .5f * width : -.5f * width;
            linkPosition.y = l + (vRenderer != null ? vRenderer.spriteEndLength : 0f);
            topLinkRenderer.transform.localPosition = linkPosition;
        }
        if (bottomLinkRenderer != null)
        {
            bottomLinkRenderer.size = new(bottomLinkLength, bottomLinkRenderer.size.y);
            linkPosition.x = bottomLink == LinkDirection.Left ? .5f * width : -.5f * width;
            linkPosition.y = 0f;
            bottomLinkRenderer.transform.localPosition = linkPosition;
        }
    }

    public void SetDistance(float value, bool ignoreLink = false)
    {
        distance = value;
        // When linked, some values can be set by the previous note
        bool isLinkedToPreviousNote = !ignoreLink && bottomLink != LinkDirection.NoLink;
        // Set sprite positions
        if (isLinkedToPreviousNote == false)
        {
            float displayDistance = DisplayDistance;
            if (hRenderer != null) hRenderer.transform.localPosition = new Vector3(displayDistance, hRenderer.transform.localPosition.y);
            if (vRenderer != null) vRenderer.transform.localPosition = new Vector3(vRenderer.transform.localPosition.x, displayDistance);
        }
        // Passed note
        if (distance < -length)
        {
            // Bullet color depends on play/miss. We only set the target here.
            target?.SetSprite(NoteTarget.DistanceState.Passed);
            target.SetDistanceRatio(0f);
        }
        // Passing note
        else if (distance < 0f)
        {
            // Same with differnet value.
            target?.SetSprite(NoteTarget.DistanceState.Current);
            target?.SetDistanceRatio(1f);
        }
        // Incoming note
        else if (distance < incomingDistance)
        {
            // Fading note bullet
            float incomingRatio = Mathf.Clamp01(distance / incomingDistance);
            if (isLinkedToPreviousNote == false)
            {
                currentColor = TintColor(baseColor, incomingTint, incomingRatio);
                SetColor(currentColor, 0f, float.PositiveInfinity);
            }
            // Fading target
            target?.SetSprite(isNext ? NoteTarget.DistanceState.Next : NoteTarget.DistanceState.Incoming);
            target?.SetColor(baseColor);
            target?.SetDistanceRatio(1f - incomingRatio);
        }
        // Note is far but visible
        else if (distance < farDistance)
        {
            // Same with different value.
            float farRatio = 1f - (distance - incomingDistance) / (farDistance - incomingDistance);
            if (isLinkedToPreviousNote == false)
            {
                currentColor = TintColor(baseColor, farTint, farRatio);
                SetColor(currentColor, 0f, float.PositiveInfinity);
            }
            target?.SetSprite(NoteTarget.DistanceState.Far);
        }
        // Note is too far to be shown
        else
        {
            // Hide note
            if (isLinkedToPreviousNote == false)
            {
                currentColor = Color.clear;
                SetColor(currentColor, 0f, float.PositiveInfinity);
            }
            target?.SetSprite(NoteTarget.DistanceState.Far);
        }
    }

    private void SetDistanceLinked(float otherDistance, float otherLength)
    {
        //distance = otherDistance + otherLength;
        float displayDistance = Round(otherDistance) + Round(otherLength) + Round(distanceOffset);
        if (hRenderer != null) hRenderer.transform.localPosition = new Vector3(displayDistance, hRenderer.transform.localPosition.y);
        if (vRenderer != null) vRenderer.transform.localPosition = new Vector3(vRenderer.transform.localPosition.x, displayDistance);
    }

    public void Play(float fromPosition, float toPosition, float accuracy)
    {
        // Cut played bits at negative positions (past part of note)
        Cut(Mathf.Min(fromPosition, 0f), Mathf.Min(toPosition, 0f));
        // Set played color at positive positions (upcoming part of note)
        currentColor = TintColor(baseColor, Color.Lerp(missedTint, playedTint, accuracy));
        SetColor(currentColor, Mathf.Max(fromPosition, 0f), Mathf.Max(toPosition, 0f));
        target?.SetColor(currentColor);
    }

    public void Miss(float fromPosition, float toPosition)
    {
        // Set color
        currentColor = TintColor(baseColor, missedTint);
        SetColor(currentColor, fromPosition, toPosition);
        target?.SetColor(currentColor);
    }

    public void FullCatch()
    {
        target?.PlayCatchAnimation();
    }

    public void Cut(float fromPosition, float toPosition, bool horizontal = true, bool vertical = true)
    {
        CutLocal(fromPosition - distance, toPosition - distance, horizontal, vertical);
    }

    public void CutLocal(float fromPosition, float toPosition, bool horizontal = true, bool vertical = true)
    {
        // Round parameters
        if (roundValues)
        {
            fromPosition = Round(fromPosition);
            toPosition = Round(toPosition);
        }
        // Horizontal render
        if (horizontal && hRenderer != null) hRenderer.Cut(fromPosition, toPosition);
        // Vertical render (with links)
        if (vertical && vRenderer != null)
        {
            vRenderer.Cut(fromPosition, toPosition);
            vRenderer.GetTipCuts(out topCut, out bottomCut);
        }
    }

    public void SetVisible(bool horizontal, bool vertical)
    {
        if (hRenderer != null) hRenderer.SetVisible(horizontal);
        if (vRenderer != null) vRenderer.SetVisible(vertical);
        if (bottomLinkRenderer != null) bottomLinkRenderer.enabled = vertical;
        if (topLinkRenderer != null) topLinkRenderer.enabled = vertical;
    }

    public void SetColorLocal(Color c, float fromPosition, float toPosition)
    {
        // Round parameters
        if (roundValues)
        {
            fromPosition = Round(fromPosition);
            toPosition = Round(toPosition);
        }
        // Horizontal render
        if (hRenderer != null) hRenderer.SetColorFromToPosition(c, fromPosition, toPosition, false, true);
        // Vertical render (with links)
        Color topColor = Color.clear;
        Color bottomColor = Color.clear;
        if (vRenderer != null)
        {
            vRenderer.SetColorFromToPosition(c, fromPosition, toPosition, false, true);
            vRenderer.GetTipColors(out bottomColor, out topColor);
        }
        if (bottomLinkRenderer != null) bottomLinkRenderer.color = bottomColor;
        if (topLinkRenderer != null) topLinkRenderer.color = topColor;
    }

    public void SetColor(Color c, float fromPosition, float toPosition)
    {
        float localFromPosition = roundValues ? Round(fromPosition - distance) : fromPosition - distance;
        float localToPosition = roundValues ? Round(toPosition - distance) : toPosition - distance;
        SetColorLocal(c, localFromPosition, localToPosition);
    }

    public void SetColor(Color c) => SetColor(c, float.NegativeInfinity, float.PositiveInfinity);

    private Color TintColor(Color c, Color tint, float blend = 1f)
    {
        float alpha = c.a;
        float ratio = blend * tint.a;
        Color tintColor = (1f - ratio) * c + ratio * tint;
        tintColor.a = alpha;
        return tintColor;
    }

    public bool TryLinkToNextNote(NoteBullet next)
    {
        // Determine if note can be link together
        if (topCut || next == null || next.bottomCut
            // Neighbour x
            || Mathf.Abs(next.transform.position.x - transform.position.x) > linkMaxDistance.x
            // Neighbour y
            || Mathf.Abs(next.transform.position.y - transform.position.y) > linkMaxDistance.y
            // Note ends when next note starts
            || next.distance > distance + length + linkMaxDistance.y)
        {
            // Notes are separated
            topLink = LinkDirection.NoLink;
        }
        else
        {
            // Notes can be linked
            float x = transform.position.x, xOther = next.transform.position.x;
            if (Mathf.Approximately(x, xOther))
            {
                // Exception for "straight" link
                topLink = LinkDirection.NoLink;
            }
            else if (x > xOther)
            {
                topLink = LinkDirection.Left;
                topLinkLength = x - xOther;
            }
            else
            {
                topLink = LinkDirection.Right;
                topLinkLength = xOther - x;
            }
        }
        // Adapt next note link accordingly
        next.bottomLink = (LinkDirection)(-(int)topLink);
        // Mark next note as next when this note is current
        next.isNext = distance < 0f;
        // If notes are link do additional matching operations and return true
        if (topLink != LinkDirection.NoLink)
        {
            // Match y positions (safer due to position rounding)
            next.SetDistanceLinked(distance, length);
            // Match two notes' color
            next.currentColor = currentColor;
            next.SetColor(currentColor);
            return true;
        }
        else
            return false;
    }

    public bool TryLinkToPreviousNote(NoteBullet previous) 
        => previous != null ? previous.TryLinkToNextNote(this) : false;


    public bool TryLinkOtherNote(NoteBullet other)
    {
        if (TryLinkToNextNote(other) == false) return TryLinkToPreviousNote(other);
        else return true;
    }

    private void SetLinkSprites()
    {
        SetLinkSprite(topLinkRenderer, topLink, topLinkLength);
        SetLinkSprite(bottomLinkRenderer, bottomLink, bottomLinkLength);
    }

    private void SetLinkSprite(SpriteRenderer linkRenderer, LinkDirection direction, float linkLength)
    {
        if (linkRenderer == null) return;
        if (direction == LinkDirection.NoLink)
            linkRenderer.enabled = false;
        else
        {
            linkRenderer.enabled = true;
            linkRenderer.flipX = direction == LinkDirection.Left;
            float xPosition = direction == LinkDirection.Left ? .5f * width : -.5f * width;
            linkRenderer.transform.localPosition = new(xPosition, linkRenderer.transform.localPosition.y);
            linkRenderer.size = new(linkLength, linkRenderer.size.y);
        }
    }
}
