using System;
using UnityEngine;

[ExecuteAlways]
public class TromboneDisplay : MonoBehaviour,
    ITromboneBlowInput, ITromboneSlideToneInput, ITrombonePressureLevelInput,
    ITromboneGrabOutput, ITromboneBlowOutput, ITromboneSlideToneOutput, ITrombonePressureLevelOutput
{
    [Flags] public enum GrabMode { AlwaysGrab = 0, GrabRadius = 1, ReleaseRadius = 2, ClickToGrab = 4, ClickToRelease = 8 }

    public bool showDebug;
    [Header("Components")]
    public Vector2Int baseSpriteSize = new Vector2Int(128, 32);
    public SpriteRenderer body;
    public SpriteRenderer slide;
    public SpriteRenderer slidebar;
    public SpriteRenderer bell;
    public SpriteRenderer grabArrow;
    [Header("Aspect")]
    public Color color = Color.white;
    public int bodyLength = 128;
    public int slideLength = 128;
    public float grabArrowDelay = -1f;
    [Header("Movement")]
    public bool roundedPosition = true;
    [Header("Slide")]
    public bool enableSlideMovement = true;
    public float toneWidth = 10f;
    public float minSlideTone = 0f;
    public float maxSlideTone = 6f;
    [Header("Pressure")]
    public bool enablePressureMovement = true;
    public float stepHeight = 11f;
    public float minPressureLevel = 0f;
    public float maxPressureLevel = 6f;
    [Header("Grab Control")]
    public GrabMode grabMode;
    public Vector2 grabOffset;
    public float grabRadius = 10f;
    public float releaseRadius = 10f;
    public float releaseForce = 0f;
    [Header("Mouse Input")]
    public bool useHandCursor = true;
    public int blowButtonNumber = 0;
    public int grabButtonNumber = 0;
    public int releaseButtonNumber = 1;

    private HandCursor hand;
    private bool mouseHover;
    private bool mouseGrab;
    private bool mouseBlow;
    private float mouseSlideTone;
    private float mousePressureLevel;
    private float grabArrowTimer;
    private bool mouseButtonUpAfterGrab;

    private bool? blowInput;
    private float? slideToneInput;
    private float? pressureLevelInput;

    public Vector2 GrabPositionOrigin => (Vector2)transform.position + grabOffset + SlideSpriteOffset * Vector2.right;
    public Vector2 GrabPosition => GrabPositionOrigin + new Vector2(slide.transform.localPosition.x, body.transform.localPosition.y);
    private float SlideSpriteOffset => bodyLength - baseSpriteSize.x;
    public float SlideBumperMinX => SlideSpriteOffset + slideLength;

    #region Input/Output interfaces
    public bool? Grab => mouseGrab;
    public bool? Blow
    {
        set => blowInput = value;
        get => blowInput == null ? mouseBlow : blowInput.Value;
    }
    public float? SlideTone
    {
        set => slideToneInput = value;
        get => slideToneInput == null ? (float.IsNaN(mouseSlideTone) ? null : mouseSlideTone) : slideToneInput;
    }
    public float? PressureLevel
    {
        set => pressureLevelInput = value;
        get => pressureLevelInput == null ? (float.IsNaN(mousePressureLevel) ? null : mousePressureLevel) : pressureLevelInput;
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GrabPosition, grabRadius);
        Gizmos.color *= .5f;
        Gizmos.DrawWireSphere(GrabPosition, releaseRadius);
    }

    private void Reset()
    {
        ClearInputs();
    }

    private void Awake()
    {
        ClearInputs();
        hand = FindObjectOfType<HandCursor>();
        if (hand == null)
        {
            Debug.LogWarning("Missing hand cursor");
            useHandCursor = false;
        }
    }

    private void Start()
    {
        UpdateAspect();
    }

    private void OnEnable()
    {
        ClearInputs();
    }

    private void OnDisable()
    {
        ClearInputs();
        // Clear internal inputs (mouse)
        mouseGrab = false;
        mouseBlow = false;
    }

    private void Update()
    {
        // Update aspect (dimensions, color...)
        UpdateAspect();
        // Cursor input
        UpdateMouseInput();
        // Position (vertical movement and slide)
        UpdatePosition();
        // Animations
        UpdateAnimations();
    }

    public void ClearInputs()
    {
        // Clear external inputs (e.g. auto)
        blowInput = null;
        slideToneInput = null;
        pressureLevelInput = null;
    }

    public void ResetDisplay()
    {
        mouseBlow = false;
        mouseGrab = false;
        mouseHover = false;
        mousePressureLevel = 0f;
        mouseSlideTone = 0f;
        UpdatePosition();
        UpdateAnimations();
    }

    public void UpdateAspect()
    {
        // Body aspect
        Vector2 bodySize = new Vector2(bodyLength, baseSpriteSize.y);
        if (body != null)
        {
            body.size = bodySize;
            body.color = color;
        }
        if (bell != null)
        {
            bell.size = bodySize;
            bell.color = color;
        }
        // Slide aspect
        Vector2 slideSize = new Vector2(slideLength, baseSpriteSize.y);
        if (slide != null)
        {
            slide.size = slideSize;
            slide.color = color;
            if (slidebar != null && SlideTone != null)
            {
                slidebar.size = slideSize + SlideTone.Value * toneWidth * Vector2.right;
                slidebar.transform.localPosition = SlideSpriteOffset * Vector2.right;
                slidebar.color = color;
            }
        }
    }

    public void UpdatePosition()
    {
        // Body position (vertical movement)
        Vector3 bodyPos = body.transform.localPosition;
        if (enablePressureMovement && PressureLevel != null && body != null)
        {
            bodyPos.y = PressureLevel.Value * stepHeight;
            if (roundedPosition) bodyPos.y = Mathf.Round(bodyPos.y);
        }
        else
        {
            bodyPos.y = 0f;
        }
        body.transform.localPosition = bodyPos;
        // Slide position (horizontal movement)
        Vector3 slidePos = slide.transform.localPosition;
        if (enableSlideMovement && SlideTone != null && slide != null)
        {
            slidePos.x = SlideTone.Value * toneWidth;
            if (roundedPosition) slidePos.x = Mathf.Round(slidePos.x);            
        }
        else
        {
            slidePos.x = 0f;
        }
        slide.transform.localPosition = slidePos;
        // Slide bar strech
        if (slidebar != null) slidebar.size = slide.size + slidePos.x * Vector2.right;
        // Cursor position and animation
        if (hand != null && useHandCursor)
        {
            if (mouseHover == true)
                hand.State |= HandCursor.CursorState.PointAtTrombone;
            else
                hand.State &= ~HandCursor.CursorState.PointAtTrombone;
            if (mouseGrab == true)
            {
                hand.State |= HandCursor.CursorState.Trombone;
                hand.handPosition = GrabPosition;
                if (releaseForce > 0f && Vector2.Distance(hand.cursorPosition, GrabPosition) > grabRadius) hand.cursorPosition = Vector2.MoveTowards(hand.cursorPosition, GrabPosition, releaseForce * Time.deltaTime);
            }
            else
            {
                hand.State &= ~HandCursor.CursorState.Trombone;
            }
        }
    }

    public void UpdateAnimations()
    {
        // Blow animation
        if (bell != null) bell.enabled = Blow.Value;
        // Arrow animation
        if (grabArrow != null)
        {
            if (mouseGrab == true)
                grabArrowTimer = 0f;
            else if(grabArrowTimer < grabArrowDelay)
                grabArrowTimer += Time.deltaTime;
            grabArrow.gameObject.SetActive(grabArrowDelay >= 0 && grabArrowTimer >= grabArrowDelay);
        }
    }

    public void UpdateMouseInput()
    {
        if (Application.isPlaying == false)
        {
            mouseGrab = false;
            mouseBlow = false;
            mouseSlideTone = 0f;
            mousePressureLevel = 0f;
            return;
        }
        // Grab
        Vector2 mouseWorldPosition;
        if (useHandCursor && hand != null)
        {
            mouseWorldPosition = hand.cursorPosition;
            if (hand.enableTromboneGrab) UpdateMouseGrabInput(mouseWorldPosition);
            else mouseGrab = false;
        }
        else
        {
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            UpdateMouseGrabInput(mouseWorldPosition);
        }
        // Tone and blow
        if (Grab.Value == true)
        {
            UpdateMouseToneInput(mouseWorldPosition);
            UpdateMouseBlowInput();
        }
    }

    private void UpdateMouseGrabInput(Vector2 mousePosition)
    {
        // Grab update
        if (mouseGrab == false)
        {
            // Always grab
            if (grabMode == GrabMode.AlwaysGrab)
                mouseGrab = true;
            // Hover
            else if (grabMode.HasFlag(GrabMode.GrabRadius) == false || Vector2.Distance(GrabPosition, mousePosition) < grabRadius)
            {
                mouseHover = true;
                // Grab
                if (grabMode.HasFlag(GrabMode.ClickToGrab) == false || Input.GetMouseButtonDown(grabButtonNumber) == true)
                {
                    mouseGrab = true;
                    mouseButtonUpAfterGrab = false;
                }
            }
            else
                mouseHover = false;
        }
        else
        {
            // Release
            if ((grabMode.HasFlag(GrabMode.ClickToRelease) == true && Input.GetMouseButtonDown(releaseButtonNumber))
                || (grabMode.HasFlag(GrabMode.ReleaseRadius) == true && Vector2.Distance(GrabPosition, mousePosition) > releaseRadius))
                mouseGrab = false;
        }
    }

    private void UpdateMouseBlowInput()
    {
        // Get mouse blow control
        if (mouseButtonUpAfterGrab == true) mouseBlow = Input.GetMouseButton(blowButtonNumber);
        else if (Input.GetMouseButtonUp(blowButtonNumber)) mouseButtonUpAfterGrab = true;
    }

    private void UpdateMouseToneInput(Vector2 mousePosition)
    {
        // Get mouse tone control
        Vector2 relativeMousePosition = mousePosition - GrabPositionOrigin;
        mouseSlideTone = relativeMousePosition.x / toneWidth;
        mousePressureLevel = relativeMousePosition.y / stepHeight;
        // Clamp mouse tone control
        mouseSlideTone = Mathf.Clamp(mouseSlideTone, minSlideTone, maxSlideTone);
        mousePressureLevel = Mathf.Clamp(mousePressureLevel, minPressureLevel, maxPressureLevel);
        // Round mouse tone control
        if (roundedPosition)
        {
            mouseSlideTone = Mathf.Round(mouseSlideTone * toneWidth) / toneWidth;
            mousePressureLevel = Mathf.Round(mousePressureLevel * stepHeight) / stepHeight;
        }
    }
}