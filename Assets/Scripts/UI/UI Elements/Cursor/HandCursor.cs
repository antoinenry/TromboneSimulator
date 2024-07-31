using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class HandCursor : BaseInput
{
    [Flags]
    public enum CursorState { Visible = 1, PointAtUI = 2, Click = 4, SecondaryClick = 8, Trombone = 16, PointAtTrombone = 32 }

    [Serializable]
    public struct HandSprites
    {
        public Sprite idle;
        public Sprite pointing;
        public Sprite clicking;
        public Sprite tromboning;

        public Sprite GetSprite(CursorState cursor)
        {
            if (cursor.HasFlag(CursorState.Visible))
            {
                if (cursor.HasFlag(CursorState.PointAtUI))
                {
                    if (cursor.HasFlag(CursorState.Click))
                        return clicking;
                    else
                        return pointing;
                }
                else if (cursor.HasFlag(CursorState.Trombone))
                {
                    return tromboning;
                }
                else if (cursor.HasFlag(CursorState.PointAtTrombone))
                {
                    return pointing;
                }    
                else
                    return idle;
            }
            return null;
        }
    }

    [Header("Components")]
    public SpriteRenderer handRenderer;
    public GraphicRaycaster menuUIRaycaster;
    public GraphicRaycaster gameUIRaycaster;
    [Header("Outputs")]
    public Vector2 cursorPosition;
    public Vector2 handPosition;
    [SerializeField] private CursorState cursorState;
    [Header("Control")]
    public int clickButtonNumber = 0;
    public int secondaryClickButtonNumber = 1;
    public string xAxisName = "Mouse X";
    public string yAxisName = "Mouse Y";
    [Header("Movement")]
    public float sensibility = 1f;
    public bool keepOnScreen = true;
    public bool enableTromboneGrab = true;
    [Header("Look")]
    public bool roundDisplayPosition;
    public HandSprites sprites;
    public string defaultSortingLayer;
    public string tromboneSortingLayer;
    [Header("Events")]
    public UnityEvent<CursorState> onStateChange;

    public CursorState State
    {
        get => cursorState;
        set
        {
            if (value != cursorState)
            {
                cursorState = value;
                onStateChange.Invoke(cursorState);
            }
        }
    }
    public bool MainClick { get; private set; }
    public bool SecondaryClick { get; private set; }

    private Camera cam;
    private StandaloneInputModule inputModule;
    private PointerEventData pointerEventData;

    protected override void Awake()
    {
        base.Awake();
        inputModule = FindObjectOfType<StandaloneInputModule>(true);
        pointerEventData = new PointerEventData(FindObjectOfType<EventSystem>(true));
        cam = Camera.main;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        inputModule.inputOverride = this;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        inputModule.inputOverride = null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override Vector2 mousePosition => enabled ? cam.WorldToScreenPoint(cursorPosition) : base.mousePosition;

    private void Update()
    {
        // Hide and confine actual cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        // Cursor movement
        Vector2 mouseMovement = new Vector2(Input.GetAxis(xAxisName), Input.GetAxis(yAxisName)) / cam.pixelHeight;
        cursorPosition += sensibility * mouseMovement;
        if (keepOnScreen)
        {
            cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0f, 2f * cam.orthographicSize * cam.aspect);
            cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0f, 2f * cam.orthographicSize);
        }
        // Cursor click
        MainClick = Input.GetMouseButton(clickButtonNumber);
        SecondaryClick = Input.GetMouseButton(secondaryClickButtonNumber);
        // Hand position
        if (cursorState.HasFlag(CursorState.Trombone) == false)
        {
            // Hand follows cursor except when grabbing trombone
            if (roundDisplayPosition)
            {
                handPosition.x = Mathf.Round(cursorPosition.x);
                handPosition.y = Mathf.Round(cursorPosition.y);
            }
            else
                handPosition = cursorPosition;
        }
        transform.position = handPosition;
        // Hand animation
        if (handRenderer != null)
        {
            CursorState newState = cursorState;
            GetCurrentState(ref newState);
            handRenderer.sprite = sprites.GetSprite(newState);
            // Sorting layer changes when grabbing trombone
            handRenderer.sortingLayerName = newState.HasFlag(CursorState.Trombone) ? tromboneSortingLayer : defaultSortingLayer;
            State = newState;
        }
    }

    private void GetCurrentState(ref CursorState state)
    {
        // Raycast from cursor position
        List<RaycastResult> results = new List<RaycastResult>();
        pointerEventData.position = mousePosition;
        // Menus
        if (MenuUI.VisibleMenuCount > 0)
            menuUIRaycaster.Raycast(pointerEventData, results);
        // GUI
        else
            gameUIRaycaster.Raycast(pointerEventData, results);
        // Process raycast hits
        state &= ~CursorState.PointAtUI;
        state &= ~CursorState.Click;
        foreach (RaycastResult r in results)
        {
            Selectable selectableTarget = r.gameObject.GetComponent<Selectable>();
            if (selectableTarget != null && selectableTarget.interactable == true)
            {
                state |= CursorState.PointAtUI;
                if (MainClick) state |= CursorState.Click;
                return;
            }
        }
    }
}