using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class NoteBullet : MonoBehaviour
{
    public enum LinkDirection { NoLink = 0, Straight = 1, Left = 2, Right = 3 }

    [Header("Components")]
    public SpriteCutter hRenderer;
    public SpriteCutter vRenderer;
    public NoteTarget target;
    [Header("Sprites")]
    public Sprite spriteSingle;
    public Sprite spriteLinkedTop;
    public Sprite spriteLinkedBottom;
    public Sprite spriteLinkedS;
    public Sprite spriteLinkedC;
    [Header("Position")]
    public float distance = 0f;
    public float distanceOffset = 0f;
    [Min(0f)] public float length = 16f;
    public bool isNext = false;
    public bool roundValues = true;
    public Vector2 linkMaxDistance = new(1f,1f);
    [Header("Colors")]
    public Color baseColor = Color.green;
    public Color farTint = Color.gray;
    public Color incomingTint = Color.white;
    public Color playedTint = Color.white;
    public Color missedTint = Color.red;
    public float farDistance = 60f;
    public float incomingDistance = 30f;

    private LinkDirection startLink;
    private LinkDirection endLink;

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
        }
        else
        {
            SetSprites();
        }
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
        // Set sprite positions
        if (hRenderer != null) hRenderer.transform.localPosition = (_distance + distanceOffset) * Vector3.right;
        if (vRenderer != null) vRenderer.transform.localPosition = (_distance + distanceOffset) * Vector3.up;
        // Passed note
        if (distance < -length)
        {
            // Bullet color depends on play/miss. We only set target here.
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
            Color incomingColor = TintColor(baseColor, incomingTint, incomingRatio);
            SetColor(incomingColor, 0f, float.PositiveInfinity);
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
            Color fadeColor = TintColor(baseColor, farTint, farRatio);
            SetColor(fadeColor, 0f, float.PositiveInfinity);
            target?.SetSprite(NoteTarget.DistanceState.Far);
        }
        // Note is too far to be shown
        else
        {
            // Hide note
            SetColor(Color.clear, 0f, float.PositiveInfinity);
            target?.SetSprite(NoteTarget.DistanceState.Far);
        }
    }

    public void Play(float fromPosition, float toPosition, float accuracy)
    {
        // Cut played bits at negative positions (past part of note)
        Cut(Mathf.Min(fromPosition, 0f), Mathf.Min(toPosition, 0f));
        // Set played color at positive positions (upcoming part of note)
        Color playColor = TintColor(baseColor, Color.Lerp(missedTint, playedTint, accuracy));
        SetColor(playColor, Mathf.Max(fromPosition, 0f), Mathf.Max(toPosition, 0f));
        target?.SetColor(playColor);
    }

    public void Miss(float fromPosition, float toPosition)
    {
        // Set color
        Color missedColor = TintColor(baseColor, missedTint);
        SetColor(missedColor, fromPosition, toPosition);
        target?.SetColor(missedColor);
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
        // Round values
        if (roundValues)
        {
            fromPosition = Mathf.Ceil(fromPosition);
            toPosition = Mathf.Ceil(toPosition);
        }
        if (horizontal && hRenderer != null) hRenderer.Cut(fromPosition, toPosition);
        if (vertical && vRenderer != null) vRenderer.Cut(fromPosition, toPosition);
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

    public bool TryLinkToNextNote(NoteBullet next)
    {
        // Determine if note can be link together
        if (next == null
            // Same pressure
            //|| !Mathf.Approximately(next.transform.position.y, transform.position.y)
            // Neighbour slide
            || Mathf.Abs(next.transform.position.x - transform.position.x) > linkMaxDistance.x
            // Note ends when next note starts
            || Mathf.Abs(next.distance - distance) > length + linkMaxDistance.y)
        {
            endLink = LinkDirection.NoLink;
            return false;
        }
        else
        {
            float x = transform.position.x, xOther = next.transform.position.x;
            if (Mathf.Approximately(x, xOther)) endLink = LinkDirection.Straight;
            else if (x > xOther) endLink = LinkDirection.Left;
            else endLink = LinkDirection.Right;
        }
        // Adapt next note link accordingly
        switch (endLink)
        {
            case LinkDirection.NoLink:
                next.startLink = LinkDirection.NoLink;
                break;
            case LinkDirection.Straight:
                next.startLink = LinkDirection.Straight;
                break;
            case LinkDirection.Left:
                next.startLink = LinkDirection.Right;
                break;
            case LinkDirection.Right:
                next.startLink = LinkDirection.Left;
                break;
        }
        // Set next note as next when this note is current
        next.isNext = distance < 0f;
        // Match notes color and return true if linked
        if (endLink != LinkDirection.NoLink)
        {
            //next.baseColor = baseColor;
            //linkLine.SetPosition(1, next.transform.position - transform.position);
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

    private void SetSprites()
    {
        // Target sprite
        //if (target) target.enabled = showTarget;
        // Bullet sprites
        switch (endLink)
        {
            case LinkDirection.NoLink:
                switch (startLink)
                {
                    case LinkDirection.NoLink:
                        SetSprites(spriteSingle, spriteSingle);
                        break;
                    case LinkDirection.Straight:
                        SetSprites(spriteSingle, spriteSingle);
                        break;
                    case LinkDirection.Left:
                        SetSprites(spriteSingle, spriteLinkedBottom);
                        break;
                    case LinkDirection.Right:
                        SetSprites(spriteSingle, spriteLinkedBottom, vFlip: true);
                        break;
                }
                break;
            case LinkDirection.Straight:
                switch (startLink)
                {
                    case LinkDirection.NoLink:
                        SetSprites(spriteSingle, spriteSingle);
                        break;
                    case LinkDirection.Straight:
                        SetSprites(spriteSingle, spriteSingle);
                        break;
                    case LinkDirection.Left:
                        SetSprites(spriteSingle, spriteLinkedBottom);
                        break;
                    case LinkDirection.Right:
                        SetSprites(spriteSingle, spriteLinkedBottom, vFlip: true);
                        break;
                }
                break;
            case LinkDirection.Left:
                switch (startLink)
                {
                    case LinkDirection.NoLink:
                        SetSprites(spriteSingle, spriteLinkedTop, vFlip: true);
                        break;
                    case LinkDirection.Straight:
                        SetSprites(spriteSingle, spriteLinkedTop, vFlip: true);
                        break;
                    case LinkDirection.Left:
                        SetSprites(spriteSingle, spriteLinkedC, vFlip: true);
                        break;
                    case LinkDirection.Right:
                        SetSprites(spriteSingle, spriteLinkedS, vFlip: true);
                        break;
                }
                break;
            case LinkDirection.Right:
                switch (startLink)
                {
                    case LinkDirection.NoLink:
                        SetSprites(spriteSingle, spriteLinkedTop);
                        break;
                    case LinkDirection.Straight:
                        SetSprites(spriteSingle, spriteLinkedTop);
                        break;
                    case LinkDirection.Left:
                        SetSprites(spriteSingle, spriteLinkedS);
                        break;
                    case LinkDirection.Right:
                        SetSprites(spriteSingle, spriteLinkedC);
                        break;
                }
                break;
        }
    }

    private void SetSprites(Sprite hSprite, Sprite vSprite, bool vFlip = false)
    {
        if (hRenderer != null) 
            hRenderer.spriteRenderer.sprite = hSprite;
        if (vRenderer != null)
        {
            vRenderer.spriteRenderer.sprite = vSprite;
            vRenderer.SetFlipX(vFlip);
        }
    }
}
